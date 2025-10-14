using System;

namespace Transfer3.Domain.Models;

public class DeviceInformation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public DeviceType MyDeviceType { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool IsCurrentDevice { get; set; }
    public string DeviceType => MyDeviceType.ToString();
}
