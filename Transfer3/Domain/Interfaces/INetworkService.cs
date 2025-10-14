using Transfer3.Domain.Models;

namespace Transfer3.Domain.Interfaces;

public interface INetworkService
{
    Task<bool> IsNetworkAvailableAsync();
    Task<string> GetLocalIPAddressAsync();
    Task<string> GetDeviceNameAsync();
    Task<bool> IsDeviceOnlineAsync(string ipAddress);
    Task StartDiscoveryAsync();
    Task StopDiscoveryAsync();
    Task<bool> RefreshDevicesAsync();
    event EventHandler<DeviceInformation> DeviceDiscovered;
    event EventHandler<string> DeviceLost;
}