using Transfer3.Services.Contracts;
using Transfer3.Services.Implementations;
using Transfer3.ViewModels;

using Microsoft.Extensions.Logging;

namespace Transfer3;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()

            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Services (Singleton for services that maintain state)
        builder.Services.AddSingleton<INetworkService, NetworkService>();
        builder.Services.AddSingleton<IDeviceInfoService, DeviceInfoService>();
        builder.Services.AddSingleton<IDeviceDiscoveryService, DeviceDiscoveryService>();
        builder.Services.AddSingleton<IFileTransferService, FileTransferService>();
        builder.Services.AddSingleton<IFilePickerService, FilePickerService>();

        // Register ViewModels (Transient - new instance each time)
        builder.Services.AddTransient<MainPageViewModel>();

        // Register Views (Transient - new instance each time)
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}