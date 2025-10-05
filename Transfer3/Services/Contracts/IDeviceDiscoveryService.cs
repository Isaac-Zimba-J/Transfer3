using System;
using Transfer3.Domain.Models;

namespace Transfer3.Services.Contracts;

/// <summary>
/// Service for discovering devices on the network.
/// uses mDNS and UDP broadcasting to find devices running the Transfer3 app.
/// </summary>

public interface IDeviceDiscoveryService
{
    /// <summary>
    /// Event triggered when a new device is discovered.
    /// </summary>
    event EventHandler<DeviceInformation>? DeviceDiscovered;

    /// <summary>
    /// Event triggered when a device is no longer reachable.
    /// </summary>
    event EventHandler<DeviceInformation>? DeviceLost;


    /// <summary>
    /// Start listening for device discovery broadcasts
    /// This makes the device discoverable by others
    /// </summary>
    Task StartDiscoveryAsync();

    /// <summary>
    /// Stop listening for device discovery broadcasts
    /// This makes the device no longer discoverable by others
    /// </summary>
    Task StopDiscoveryAsync();

    /// <summary>
    /// Broadcast a discovery message to find other devices
    /// this actively searches for other devices
    /// </summary>
    Task BroadcastDiscoveryAsync();

    /// <summary>
    /// Get a list of currently discovered devices
    /// </summary>
    Task<List<DeviceInformation>> GetDiscoveredDevicesAsync();

    /// <summary>
    /// Get the current device's information
    /// </summary>
    Task<DeviceInformation> GetCurrentDeviceInfoAsync();

    /// <summary>
    /// Clear the list of discovered devices
    /// </summary>
    Task ClearDiscoveredDevicesAsync();

    /// <summary>
    /// Check if the discovery service is running
    /// </summary>
    bool IsDiscovering
    {
        get;

    }
}