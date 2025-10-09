using System;
using Transfer3.Domain.Enums;

namespace Transfer3.Domain.Models;

public class FileTransferInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }           // keep as long
    public TransferStatus Status { get; set; }
    public DeviceInformation DestinationDevice { get; set; }
    public DateTime StartTime { get; set; }
    public double Progress { get; set; }         // optional, track % progress
    public DateTime? EndTime { get; set; }       // optional, track when done
    public string ErrorMessage { get; internal set; } = string.Empty;
    public long BytesTransferred { get; internal set; }
    public double TransferSpeed { get; internal set; }
}
