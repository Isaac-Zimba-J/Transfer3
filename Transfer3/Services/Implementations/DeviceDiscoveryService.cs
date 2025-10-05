using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Transfer3.Domain.Enums;
using Transfer3.Domain.Models;
using Transfer3.Services.Contracts;

namespace Transfer3.Services.Implementations;

/// <summary>
/// Discovers devices on the local network using UDP broadcasting
/// How it works:
/// 1. Listens on a UDP port for discovery broadcasts
/// 2. Periodically broadcasts presence to the network
/// 3. Maintains a list of discovered devices
/// 4. Removes devices that haven't been seen recently
/// </summary>


public class DeviceDiscoveryService : IDeviceDiscoveryService
{
    private const int DiscoveryPort = 48888;
    private const int HeartbeatIntervalSeconds = 5;
    private const int DeviceTimeoutSeconds = 15;

    private readonly INetworkService _networkService;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly ConcurrentDictionary<string, DeviceInformation> _discoveredDevices = new();

    private UdpClient? _udpListener;
    private CancellationTokenSource? _discoveryToken;
    private DeviceInformation? _currentDevice;
    private bool _isDiscovering;

    public event EventHandler<DeviceInformation>? DeviceDiscovered;
    public event EventHandler<DeviceInformation>? DeviceLost;

    public bool IsDiscovering => _isDiscovering;

    public DeviceDiscoveryService(
        INetworkService networkService,
        IDeviceInfoService deviceInfoService)
    {
        _networkService = networkService;
        _deviceInfoService = deviceInfoService;
    }



    public Task ClearDiscoveredDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DeviceInformation> GetCurrentDeviceInfoAsync()
    {

        if (_currentDevice != null)
            return _currentDevice;

        var localIp = await _networkService.GetLocalIpAddressAsync();
        var deviceName = await _deviceInfoService.GetDeviceNameAsync();
        var deviceType = await _deviceInfoService.GetDeviceTypeAsync();
        var deviceId = await _deviceInfoService.GetDeviceIdAsync();

        _currentDevice = new DeviceInformation
        {
            Id = deviceId,
            Name = deviceName,
            IpAddress = localIp,
            Port = DiscoveryPort,
            MyDeviceType = (DeviceType)deviceType,
            IsOnline = true,
            IsCurrentDevice = true,
            LastSeen = DateTime.UtcNow
        };

        return _currentDevice;
    }

    public async Task<List<DeviceInformation>> GetDiscoveredDevicesAsync()
    {
        await Task.CompletedTask;
        return _discoveredDevices.Values
            .Where(d => !d.IsCurrentDevice && d.IsOnline)
            .OrderByDescending(d => d.LastSeen)
            .ToList();
    }

    public async Task StartDiscoveryAsync()
    {
        if (_isDiscovering)
            return;

        _isDiscovering = true;
        _discoveryToken = new CancellationTokenSource();


        // get current device information
        _currentDevice = await GetCurrentDeviceInfoAsync();

        // start listening for broadcasts
        await StartDiscoveryAsync();

        // Start heartbeat broadcasting
        _ = Task.Run(() => BroadcastHeartbeatLoopAsync(_discoveryToken.Token));

        // Start device timeout monitoring
        _ = Task.Run(() => MonitorDeviceTimeoutsAsync(_discoveryToken.Token));

        // Send initial broadcast
        await BroadcastPresenceAsync();

    }

    public async Task StopDiscoveryAsync()
    {
        if (!_isDiscovering)
            return;

        _isDiscovering = false;
        _discoveryToken?.Cancel();

        _udpListener?.Close();
        _udpListener?.Dispose();
        _udpListener = null;

        _discoveredDevices.Clear();

        await Task.CompletedTask;
    }
    public async Task BroadcastPresenceAsync()
    {
        if (_currentDevice == null)
            return;

        try
        {
            var broadcastAddress = await _networkService.GetBroadcastAddressAsync();
            var message = CreateDiscoveryMessage(_currentDevice);
            var data = Encoding.UTF8.GetBytes(message);

            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            var endpoint = new IPEndPoint(IPAddress.Parse(broadcastAddress), DiscoveryPort);
            await udpClient.SendAsync(data, data.Length, endpoint);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Broadcast error: {ex.Message}");
        }
    }

    private async Task StartListeningAsync()
    {
        try
        {
            _udpListener = new UdpClient(DiscoveryPort);
            _udpListener.EnableBroadcast = true;

            _ = Task.Run(async () => await ListenForBroadcastsAsync(_discoveryToken!.Token));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start listener: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task ListenForBroadcastsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _udpListener != null)
        {
            try
            {
                var result = await _udpListener.ReceiveAsync(token);
                var message = Encoding.UTF8.GetString(result.Buffer);

                await ProcessDiscoveryMessageAsync(message, result.RemoteEndPoint.Address.ToString());
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Listen error: {ex.Message}");
            }
        }
    }

    private async Task ProcessDiscoveryMessageAsync(string message, string senderIp)
    {
        try
        {
            var deviceInfo = JsonSerializer.Deserialize<DeviceInformation>(message);
            if (deviceInfo == null)
                return;

            // Ignore messages from ourselves
            if (deviceInfo.Id == _currentDevice?.Id)
                return;

            // Update IP address to actual sender IP (in case of NAT)
            deviceInfo.IpAddress = senderIp;
            deviceInfo.IsOnline = true;
            deviceInfo.LastSeen = DateTime.UtcNow;

            var isNew = !_discoveredDevices.ContainsKey(deviceInfo.Id);
            _discoveredDevices[deviceInfo.Id] = deviceInfo;

            if (isNew)
            {
                DeviceDiscovered?.Invoke(this, deviceInfo);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Process message error: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task BroadcastHeartbeatLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(HeartbeatIntervalSeconds), token);
                await BroadcastPresenceAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Heartbeat error: {ex.Message}");
            }
        }
    }

    private async Task MonitorDeviceTimeoutsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(HeartbeatIntervalSeconds), token);

                var now = DateTime.UtcNow;
                var timedOutDevices = _discoveredDevices.Values
                    .Where(d => (now - d.LastSeen).TotalSeconds > DeviceTimeoutSeconds)
                    .ToList();

                foreach (var device in timedOutDevices)
                {
                    device.IsOnline = false;
                    DeviceLost?.Invoke(this, device);
                    _discoveredDevices.TryRemove(device.Id, out _);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout monitor error: {ex.Message}");
            }
        }
    }

    private string CreateDiscoveryMessage(DeviceInformation device)
    {
        return JsonSerializer.Serialize(device);
    }

}