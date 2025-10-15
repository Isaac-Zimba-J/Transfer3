using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Transfer3.Domain.Enums;
using Transfer3.Domain.Models;
using Transfer3.Services.Contracts;

namespace Transfer3.Services.Implementations;

public class FileTransferService : IFileTransferService
{
    private const int TransferPort = 48889;
    private const int BufferSize = 8192; // 8KB chunks

    private readonly ConcurrentDictionary<string, FileTransferInfo> _activeTransfers = new();
    private readonly List<FileTransferInfo> _transferHistory = new();
    private readonly ConcurrentDictionary<string, FileTransferInfo> _pendingRequests = new();

    private TcpListener? _tcpListener;
    private CancellationTokenSource? _listenerToken;
    private bool _isListening;

    public event EventHandler<FileTransferInfo>? TransferProgressChanged;
    public event EventHandler<FileTransferInfo>? TransferCompleted;
    public event EventHandler<FileTransferInfo>? IncomingTransferRequest;

    public async Task StartListeningAsync(int port = TransferPort)
    {
        if (_isListening)
            return;

        _listenerToken = new CancellationTokenSource();
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        _isListening = true;

        _ = Task.Run(() => AcceptConnectionsAsync(_listenerToken.Token));

        await Task.CompletedTask;
    }

    public async Task StopListeningAsync()
    {
        if (!_isListening)
            return;

        _isListening = false;
        _listenerToken?.Cancel();
        _tcpListener?.Stop();
        _tcpListener = null;

        await Task.CompletedTask;
    }

    public async Task<FileTransferInfo> SendFileAsync(string filePath, DeviceInformation targetDevice)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var fileInfo = new FileInfo(filePath);
        var transfer = new FileTransferInfo
        {
            FileName = fileInfo.Name,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            Status = TransferStatus.Pending,
            DestinationDevice = targetDevice,
            StartTime = DateTime.UtcNow
        };

        _activeTransfers[transfer.Id] = transfer;

        // Start transfer in background
        _ = Task.Run(async () => await ExecuteSendAsync(transfer, targetDevice));

        return await Task.FromResult(transfer);
    }

    public async Task AcceptTransferAsync(string transferId, string savePath)
    {
        if (!_pendingRequests.TryGetValue(transferId, out var transfer))
            return;

        transfer.FilePath = Path.Combine(savePath, transfer.FileName);
        transfer.Status = TransferStatus.InProgress;

        _pendingRequests.TryRemove(transferId, out _);
        _activeTransfers[transferId] = transfer;

        // The actual receiving is handled by the connection that's already established
        await Task.CompletedTask;
    }

    public async Task RejectTransferAsync(string transferId)
    {
        if (!_pendingRequests.TryRemove(transferId, out var transfer))
            return;

        transfer.Status = TransferStatus.Cancelled;
        transfer.EndTime = DateTime.UtcNow;

        _transferHistory.Add(transfer);
        TransferCompleted?.Invoke(this, transfer);

        await Task.CompletedTask;
    }

    public async Task CancelTransferAsync(string transferId)
    {
        if (!_activeTransfers.TryGetValue(transferId, out var transfer))
            return;

        transfer.Status = TransferStatus.Cancelled;
        transfer.EndTime = DateTime.UtcNow;

        _activeTransfers.TryRemove(transferId, out _);
        _transferHistory.Add(transfer);

        TransferCompleted?.Invoke(this, transfer);

        await Task.CompletedTask;
    }

    public async Task<List<FileTransferInfo>> GetActiveTransfersAsync()
    {
        await Task.CompletedTask;
        return _activeTransfers.Values.ToList();
    }

    public async Task<List<FileTransferInfo>> GetTransferHistoryAsync()
    {
        await Task.CompletedTask;
        return _transferHistory.ToList();
    }


    private async Task AcceptConnectionsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _tcpListener != null)
        {
            try
            {
                var client = await _tcpListener.AcceptTcpClientAsync(token);
                _ = Task.Run(() => HandleIncomingConnectionAsync(client, token));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Accept connection error: {ex.Message}");
            }
        }
    }

    private async Task HandleIncomingConnectionAsync(TcpClient client, CancellationToken token)
    {
        try
        {
            using (client)
            {
                var stream = client.GetStream();

                // Read metadata
                var metadataBytes = await ReadExactAsync(stream, 4, token);
                var metadataLength = BitConverter.ToInt32(metadataBytes, 0);

                var metadataJson = await ReadExactAsync(stream, metadataLength, token);
                var metadata = Encoding.UTF8.GetString(metadataJson);
                var transfer = JsonSerializer.Deserialize<FileTransferInfo>(metadata);

                if (transfer == null)
                    return;

                // Store as pending request
                _pendingRequests[transfer.Id] = transfer;
                IncomingTransferRequest?.Invoke(this, transfer);

                // Wait for user acceptance (with timeout)
                var timeout = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < timeout && !token.IsCancellationRequested)
                {
                    if (_activeTransfers.ContainsKey(transfer.Id))
                    {
                        // Accepted - send acknowledgment
                        await stream.WriteAsync(Encoding.UTF8.GetBytes("ACCEPT"), token);
                        await ReceiveFileDataAsync(stream, transfer, token);
                        return;
                    }

                    if (!_pendingRequests.ContainsKey(transfer.Id))
                    {
                        // Rejected
                        await stream.WriteAsync(Encoding.UTF8.GetBytes("REJECT"), token);
                        return;
                    }

                    await Task.Delay(100, token);
                }

                // Timeout
                await stream.WriteAsync(Encoding.UTF8.GetBytes("TIMEOUT"), token);
                _pendingRequests.TryRemove(transfer.Id, out _);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Handle connection error: {ex.Message}");
        }
    }

    private async Task ExecuteSendAsync(FileTransferInfo transfer, DeviceInformation targetDevice)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(targetDevice.IpAddress, TransferPort);

            var stream = client.GetStream();

            // Send metadata
            var metadata = JsonSerializer.Serialize(transfer);
            var metadataBytes = Encoding.UTF8.GetBytes(metadata);
            var lengthBytes = BitConverter.GetBytes(metadataBytes.Length);

            await stream.WriteAsync(lengthBytes);
            await stream.WriteAsync(metadataBytes);

            // Wait for response
            var responseBuffer = new byte[10];
            var bytesRead = await stream.ReadAsync(responseBuffer);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            if (response != "ACCEPT")
            {
                transfer.Status = TransferStatus.Cancelled;
                transfer.ErrorMessage = "Transfer rejected by recipient";
                CompleteTransfer(transfer);
                return;
            }

            // Send file data
            transfer.Status = TransferStatus.InProgress;
            transfer.StartTime = DateTime.UtcNow;

            using var fileStream = File.OpenRead(transfer.FilePath);
            var buffer = new byte[BufferSize];
            int read;
            var startTime = DateTime.UtcNow;

            while ((read = await fileStream.ReadAsync(buffer)) > 0)
            {
                if (transfer.Status == TransferStatus.Cancelled)
                    break;

                await stream.WriteAsync(buffer.AsMemory(0, read));
                transfer.BytesTransferred += read;

                // Calculate speed
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                if (elapsed > 0)
                {
                    transfer.TransferSpeed = transfer.BytesTransferred / elapsed;
                }

                TransferProgressChanged?.Invoke(this, transfer);
            }

            if (transfer.Status != TransferStatus.Cancelled)
            {
                transfer.Status = TransferStatus.Completed;
            }

            CompleteTransfer(transfer);
        }
        catch (Exception ex)
        {
            transfer.Status = TransferStatus.Failed;
            transfer.ErrorMessage = ex.Message;
            CompleteTransfer(transfer);
        }
    }

    private async Task ReceiveFileDataAsync(NetworkStream stream, FileTransferInfo transfer, CancellationToken token)
    {
        try
        {
            transfer.StartTime = DateTime.UtcNow;

            using var fileStream = File.Create(transfer.FilePath);
            var buffer = new byte[BufferSize];
            var startTime = DateTime.UtcNow;

            while (transfer.BytesTransferred < transfer.FileSize && !token.IsCancellationRequested)
            {
                var toRead = (int)Math.Min(BufferSize, transfer.FileSize - transfer.BytesTransferred);
                var read = await stream.ReadAsync(buffer.AsMemory(0, toRead), token);

                if (read == 0)
                    break;

                await fileStream.WriteAsync(buffer.AsMemory(0, read), token);
                transfer.BytesTransferred += read;

                // Calculate speed
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                if (elapsed > 0)
                {
                    transfer.TransferSpeed = transfer.BytesTransferred / elapsed;
                }

                TransferProgressChanged?.Invoke(this, transfer);
            }

            if (transfer.BytesTransferred == transfer.FileSize)
            {
                transfer.Status = TransferStatus.Completed;
            }
            else
            {
                transfer.Status = TransferStatus.Failed;
                transfer.ErrorMessage = "Incomplete transfer";
            }

            CompleteTransfer(transfer);
        }
        catch (Exception ex)
        {
            transfer.Status = TransferStatus.Failed;
            transfer.ErrorMessage = ex.Message;
            CompleteTransfer(transfer);
        }
    }

    private void CompleteTransfer(FileTransferInfo transfer)
    {
        transfer.EndTime = DateTime.UtcNow;
        _activeTransfers.TryRemove(transfer.Id, out _);
        _transferHistory.Add(transfer);
        TransferCompleted?.Invoke(this, transfer);
    }

    private async Task<byte[]> ReadExactAsync(NetworkStream stream, int count, CancellationToken token)
    {
        var buffer = new byte[count];
        var offset = 0;

        while (offset < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), token);
            if (read == 0)
                throw new IOException("Connection closed");
            offset += read;
        }

        return buffer;
    }

    private async Task SendFileAsync(Stream fileStream, long fileSize, string fileName, IPEndPoint endpoint, string transferId)
    {
        var buffer = new byte[8192];
        long totalBytesSent = 0;
        var startTime = DateTime.UtcNow;

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
        using var networkStream = tcpClient.GetStream();

        while (totalBytesSent < fileSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            await networkStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesSent += bytesRead;

            var progress = (double)totalBytesSent / fileSize * 100;
            var transferTime = (DateTime.UtcNow - startTime).TotalSeconds;
            var speed = totalBytesSent / transferTime; // bytes per second

            UpdateTransferProgress(transferId, progress, speed);
            await Task.Delay(100); // Prevent UI flooding
        }
    }

    private void UpdateTransferProgress(string transferId, double progress, double speed)
    {
        if (_activeTransfers.TryGetValue(transferId, out var transfer))
        {
            transfer.Progress = Math.Min(progress, 100);
            transfer.TransferSpeed = speed;
            TransferProgressChanged?.Invoke(this, transfer);
        }
    }
}
