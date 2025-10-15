using System;
using Transfer3.Domain.Enums;

using Transfer3.Services.Contracts;

namespace Transfer3.Services.Implementations;

/// <summary>
/// Platform-agnostic device info service
/// Uses .NET MAUI's DeviceInfo and Preferences APIs
/// </summary>
public class DeviceInfoService : IDeviceInfoService
{
    private const string DeviceIdKey = "DeviceUniqueId";

    public async Task<string> GetDeviceNameAsync()
    {
        await Task.CompletedTask;

        // Try to get custom name from preferences first
        var customName = Preferences.Get("DeviceName", string.Empty);
        if (!string.IsNullOrEmpty(customName))
            return customName;

        // Fall back to device info
        var deviceName = DeviceInfo.Current.Name;
        if (!string.IsNullOrEmpty(deviceName))
            return deviceName;

        // Last resort: generate a name based on device type
        var deviceType = GetDeviceType();
        return $"{deviceType} Device";
    }

    public async Task<MyDeviceType> GetDeviceTypeAsync()
    {
        await Task.CompletedTask;
        return GetDeviceType();
    }

    public async Task<string> GetDeviceIdAsync()
    {
        await Task.CompletedTask;

        // Check if we already have a stored ID
        var storedId = Preferences.Get(DeviceIdKey, string.Empty);
        if (!string.IsNullOrEmpty(storedId))
            return storedId;

        // Generate a new unique ID and store it
        var newId = Guid.NewGuid().ToString();
        Preferences.Set(DeviceIdKey, newId);
        return newId;
    }

    public async Task<string> GetOSVersionAsync()
    {
        await Task.CompletedTask;
        return $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
    }


    private MyDeviceType GetDeviceType()
    {
        var platform = DeviceInfo.Current.Platform;

        if (platform == DevicePlatform.Android)
            return MyDeviceType.Android;
        if (platform == DevicePlatform.iOS)
            return MyDeviceType.iOS;
        if (platform == DevicePlatform.MacCatalyst || platform == DevicePlatform.macOS)
            return MyDeviceType.macOS;
        if (platform == DevicePlatform.WinUI)
            return MyDeviceType.Windows;

        return MyDeviceType.Unknown;
    }

}