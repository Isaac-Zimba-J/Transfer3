using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Transfer3.Domain.Enums;
using Transfer3.Domain.Models;
using Transfer3.Services.Contracts;

namespace Transfer3.ViewModels;

public partial class MainPageViewModel : ObservableObject
{

    private readonly IDeviceDiscoveryService _discoveryService;
    private readonly IFileTransferService _transferService;
    private readonly IFilePickerService _filePickerService;
    private readonly INetworkService _networkService;

    [ObservableProperty]
    private DeviceInformation? _currentDevice;
    [ObservableProperty]
    private bool _isDiscovering;
    [ObservableProperty]
    private bool _isConnected;
    [ObservableProperty]

    private string _statusMessage = "Not connected";
    [ObservableProperty]
    private DeviceInformation? _selectedDevice;

    public ObservableCollection<DeviceInformation> DiscoveredDevices { get; } = new();
    public ObservableCollection<FileTransferInfo> ActiveTransfers { get; } = new();
    public ObservableCollection<FileTransferInfo> TransferHistory { get; } = new();


    public MainPageViewModel(
            IDeviceDiscoveryService discoveryService,
            IFileTransferService transferService,
            IFilePickerService filePickerService,
            INetworkService networkService)
    {
        _discoveryService = discoveryService;
        _transferService = transferService;
        _filePickerService = filePickerService;
        _networkService = networkService;

        // Subscribe to events
        _discoveryService.DeviceDiscovered += OnDeviceDiscovered;
        _discoveryService.DeviceLost += OnDeviceLost;
        _transferService.TransferProgressChanged += OnTransferProgressChanged;
        _transferService.TransferCompleted += OnTransferCompleted;
        _transferService.IncomingTransferRequest += OnIncomingTransferRequest;
        _networkService.ConnectivityChanged += OnConnectivityChanged;
    }


    // Commands
    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            IsConnected = await _networkService.IsConnectedAsync();
            if (!IsConnected)
            {
                StatusMessage = "No network connection";
                return;
            }

            CurrentDevice = await _discoveryService.GetCurrentDeviceInfoAsync();
            StatusMessage = $"Ready - {CurrentDevice.Name}";

            // start services
            await _transferService.StartListeningAsync();
            await StartDiscoveryAsync();

        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StartDiscoveryAsync()
    {
        if (IsDiscovering)
            return;

        try
        {
            await _discoveryService.StartDiscoveryAsync();
            IsDiscovering = true;
            StatusMessage = "Searching for devices...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Discovery error: {ex.Message}";
        }
    }


    [RelayCommand]
    private async Task StopDiscoveryAsync()
    {
        if (!IsDiscovering)
            return;

        try
        {
            await _discoveryService.StopDiscoveryAsync();
            IsDiscovering = false;
            DiscoveredDevices.Clear();
            StatusMessage = "Discovery stopped";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshDevicesAsync()
    {
        try
        {
            await _discoveryService.BroadcastPresenceAsync();
            StatusMessage = "Searching for devices...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
    [RelayCommand(CanExecute = nameof(CanSendFile))]
    private async Task SendFileAsync()
    {
        if (SelectedDevice == null)
            return;

        try
        {
            var filePath = await _filePickerService.PickFileAsync();
            if (string.IsNullOrEmpty(filePath))
                return;

            var transfer = await _transferService.SendFileAsync(filePath, SelectedDevice);
            ActiveTransfers.Add(transfer);
            StatusMessage = $"Sending {transfer.FileName}...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Send error: {ex.Message}";
        }
    }

    private bool CanSendFile() => SelectedDevice != null && IsConnected;

    [RelayCommand]
    private async Task CancelTransferAsync(string transferId)
    {
        try
        {
            await _transferService.CancelTransferAsync(transferId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Cancel error: {ex.Message}";
        }
    }

    private void OnDeviceDiscovered(object? sender, DeviceInformation device)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!DiscoveredDevices.Any(d => d.Id == device.Id))
            {
                DiscoveredDevices.Add(device);
                StatusMessage = $"Found {device.Name}";
            }
        });
    }

    private void OnDeviceLost(object? sender, DeviceInformation device)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = DiscoveredDevices.FirstOrDefault(d => d.Id == device.Id);
            if (existing != null)
            {
                DiscoveredDevices.Remove(existing);
                StatusMessage = $"{device.Name} disconnected";
            }
        });
    }

    private void OnTransferProgressChanged(object? sender, FileTransferInfo transfer)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = ActiveTransfers.FirstOrDefault(t => t.Id == transfer.Id);
            if (existing != null)
            {
                var index = ActiveTransfers.IndexOf(existing);
                ActiveTransfers[index] = transfer;
            }

            StatusMessage = $"{transfer.FileName}: {transfer.Progress:F1}%";
        });
    }

    private void OnTransferCompleted(object? sender, FileTransferInfo transfer)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = ActiveTransfers.FirstOrDefault(t => t.Id == transfer.Id);
            if (existing != null)
            {
                ActiveTransfers.Remove(existing);
            }

            TransferHistory.Insert(0, transfer);

            if (transfer.Status == TransferStatus.Completed)
            {
                StatusMessage = $"{transfer.FileName} completed";
            }
            else
            {
                StatusMessage = $"{transfer.FileName} {transfer.Status.ToString().ToLower()}";
            }
        });
    }

    private void OnIncomingTransferRequest(object? sender, FileTransferInfo transfer)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var accept = await Application.Current!.MainPage!.DisplayAlert(
                "Incoming File",
                $"Receive {transfer.FileName} ({FormatFileSize(transfer.FileSize)}) from {transfer.DestinationDevice?.Name}?",
                "Accept",
                "Reject");

            if (accept)
            {
                var savePath = await _filePickerService.PickFolderAsync();
                if (!string.IsNullOrEmpty(savePath))
                {
                    await _transferService.AcceptTransferAsync(transfer.Id, savePath);
                    ActiveTransfers.Add(transfer);
                    StatusMessage = $"Receiving {transfer.FileName}...";
                }
                else
                {
                    await _transferService.RejectTransferAsync(transfer.Id);
                }
            }
            else
            {
                await _transferService.RejectTransferAsync(transfer.Id);
            }
        });
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsConnected = isConnected;
            StatusMessage = isConnected ? "Connected" : "No network connection";

            if (!isConnected && IsDiscovering)
            {
                _ = StopDiscoveryAsync();
            }
        });
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    partial void OnSelectedDeviceChanged(DeviceInformation? value)
    {
        // Update CanExecute for SendFileCommand
        SendFileCommand.NotifyCanExecuteChanged();
    }

}
