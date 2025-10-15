using System;

namespace Transfer3.Services.Contracts;

/// <summary>
/// Service for network-related operations.
/// </summary>

public interface INetworkService
{
    /// <summary>
    /// Check if the network is available.
    /// </summary>
    /// <returns>True if the network is available, false otherwise.</returns>
    event EventHandler<bool>? ConnectivityChanged;

    /// <summary>
    /// Check if the network is available.
    /// </summary>
    Task<bool> IsConnectedAsync();

    /// <summary>
    /// Get the current IP address of the device.
    /// </summary>
    Task<string> GetLocalIpAddressAsync();

    /// <summary>
    /// Get the subnet mask of the current network.
    /// </summary>
    Task<string> GetSubnetMaskAsync();

    /// <summary>
    /// Get the broadcast address of the current network.
    /// </summary>
    Task<string> GetBroadcastAddressAsync();

    /// <summary>
    /// Check if the Wi-Fi hotspot is enabled.
    /// </summary>
    Task<bool> IsWifiHotspotEnabled();
}
