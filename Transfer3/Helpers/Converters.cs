using System;
using System.Globalization;
using Transfer3.Domain.Enums;
using Transfer3.Domain.Models;

namespace Transfer3.Helpers;

/// <summary>
/// Converts boolean to inverse boolean (true -> false, false -> true)
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

/// <summary>
/// Converts boolean to color (true = Green, false = Red)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
            return Colors.Green;
        return Colors.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts device type to emoji icon
/// </summary>
public class DeviceTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MyDeviceType deviceType)
        {
            return deviceType switch
            {
                MyDeviceType.Android => "📱",
                MyDeviceType.iOS => "📱",
                MyDeviceType.Windows => "💻",
                MyDeviceType.macOS => "💻",
                _ => "❓"
            };
        }
        return "❓";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts percentage (0-100) to decimal (0-1) for ProgressBar
/// </summary>
public class PercentToDecimalConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
            return percentage / 100.0;
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double decimal_value)
            return decimal_value * 100.0;
        return 0.0;
    }
}

/// <summary>
/// Converts transfer speed (bytes/sec) to human-readable format
/// </summary>
public class SpeedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double speed && speed > 0)
        {
            string[] units = { "B/s", "KB/s", "MB/s", "GB/s" };
            int order = 0;

            while (speed >= 1024 && order < units.Length - 1)
            {
                order++;
                speed /= 1024;
            }

            return $"{speed:0.##} {units[order]}";
        }
        return "0 B/s";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts transfer status to color
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransferStatus status)
        {
            return status switch
            {
                TransferStatus.Completed => Colors.Green,
                TransferStatus.Failed => Colors.Red,
                TransferStatus.Cancelled => Colors.Orange,
                TransferStatus.InProgress => Colors.Blue,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts transfer status to bool for ActivityIndicator (shows spinner only during active transfers)
/// </summary>
public class StatusToActivityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransferStatus status)
        {
            return status == TransferStatus.InProgress;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts transfer speed and remaining bytes to estimated time remaining
/// </summary>
public class TimeRemainingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FileTransferInfo transfer && transfer.TransferSpeed > 0 && transfer.Progress < 100)
        {
            var remainingBytes = transfer.FileSize * (100 - transfer.Progress) / 100;
            var remainingSeconds = remainingBytes / transfer.TransferSpeed;

            if (remainingSeconds > 3600) // More than 1 hour
                return $"{remainingSeconds / 3600:F0}h {(remainingSeconds % 3600) / 60:F0}m";
            else if (remainingSeconds > 60) // More than 1 minute
                return $"{remainingSeconds / 60:F0}m {remainingSeconds % 60:F0}s";
            else if (remainingSeconds > 0)
                return $"{remainingSeconds:F0}s";
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}