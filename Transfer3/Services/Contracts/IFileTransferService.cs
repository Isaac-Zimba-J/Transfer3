using System;
using Transfer3.Domain.Models;

namespace Transfer3.Services.Contracts;

/// <summary>
/// 
/// </summary>
public interface IFileTransferService
{// Events
    event EventHandler<FileTransferInfo>? TransferProgressChanged;
    event EventHandler<FileTransferInfo>? TransferCompleted;
    event EventHandler<FileTransferInfo>? IncomingTransferRequest;

    // Start/Stop listening for incoming transfers
    Task StartListeningAsync(int port = 48889);
    Task StopListeningAsync();

    // Send a file
    Task<FileTransferInfo> SendFileAsync(string filePath, DeviceInformation targetDevice);

    // Accept or reject incoming transfers
    Task AcceptTransferAsync(string transferId, string savePath);
    Task RejectTransferAsync(string transferId);

    // Cancel an active transfer
    Task CancelTransferAsync(string transferId);

    // Get lists of transfers
    Task<List<FileTransferInfo>> GetActiveTransfersAsync();
    Task<List<FileTransferInfo>> GetTransferHistoryAsync();

}
