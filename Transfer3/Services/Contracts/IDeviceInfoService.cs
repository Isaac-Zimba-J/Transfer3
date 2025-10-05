using System;
using Transfer3.Domain.Enums;

namespace Transfer3.Services.Contracts;

/// <summary>
/// Service for retrieving device information.
/// </summary>

public interface IDeviceInfoService
{

    /// <summary>
    /// Get the device's name.
    /// </summary>

    Task<string> GetDeviceNameAsync();

    /// <summary>
    /// Get device type (e.g., Mobile, Desktop).
    /// </summary>
    Task<MyDeviceType> GetDeviceTypeAsync();

    /// <summary>
    /// get device id (unique identifier)
    /// </summary>
    Task<string> GetDeviceIdAsync();

    /// <summary>
    /// Get the operating system version.
    /// </summary>
    Task<string> GetOSVersionAsync();

}
