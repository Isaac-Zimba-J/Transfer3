using System;

namespace Transfer3.Helpers;

public static class PermissionHelper
{
    /// <summary>
    /// Request all necessary permissions for the app
    /// </summary>
    public static async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            // Request storage permissions
            var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (storageStatus != PermissionStatus.Granted)
            {
                storageStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            // Request read storage permission
            var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (readStatus != PermissionStatus.Granted)
            {
                readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            // On Android 13+, request media permissions
            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                var mediaStatus = await Permissions.CheckStatusAsync<Permissions.Media>();
                if (mediaStatus != PermissionStatus.Granted)
                {
                    mediaStatus = await Permissions.RequestAsync<Permissions.Media>();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Permission request error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if all required permissions are granted
    /// </summary>
    public static async Task<bool> CheckPermissionsAsync()
    {
        try
        {
            var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            return storageStatus == PermissionStatus.Granted &&
                   readStatus == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }
}
