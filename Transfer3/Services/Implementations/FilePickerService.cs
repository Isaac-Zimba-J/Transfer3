using System;
using Transfer3.Domain.Models;
using Transfer3.Services.Contracts;

namespace Transfer3.Services.Implementations;

public class FilePickerService : IFilePickerService
{
    public async Task<string?> PickFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Selecct a file to send" });
            return result?.FullPath;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"File picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> PickFolderAsync()
    {
        try
        {
            // MAUI doesn't have built-in folder picker yet
            // We'll use FolderPicker on Windows and default Downloads on mobile

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // Use Windows folder picker via platform-specific code
                return await PickFolderWindowsAsync();
            }
            else if (DeviceInfo.Platform == DevicePlatform.macOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            {
                // Use macOS folder picker
                return await PickFolderMacAsync();
            }
            else
            {
                // Mobile: use Downloads directory
                return GetDownloadsPath();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<string>> PickMultipleFileAsync()
    {
        try
        {
            var results = await FilePicker.Default.PickMultipleAsync(new PickOptions { PickerTitle = "Select files to send" });
            return results?.Select(r => r.FullPath).ToList() ?? new List<string>();
        }
        catch (System.Exception ex)
        {

            System.Diagnostics.Debug.WriteLine($"File picker error: {ex.Message}");
            return new List<string>();
        }
    }

    private async Task<string?> PickFolderWindowsAsync()
    {
        // This would require platform-specific implementation
        // For now, return Documents folder
        await Task.CompletedTask;
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private async Task<string?> PickFolderMacAsync()
    {
        // This would require platform-specific implementation
        // For now, return Downloads folder
        await Task.CompletedTask;
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    private string GetDownloadsPath()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // Android Downloads directory
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Download");
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // iOS Documents directory (will be accessible in Files app)
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
