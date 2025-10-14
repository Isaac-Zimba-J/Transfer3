using Transfer3.ViewModels;
using Transfer3.Services.Implementations;
using Transfer3.Services.Contracts;

namespace Transfer3;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var networkService = new NetworkService();
		var deviceInfoService = new DeviceInfoService();
		var filePickerService = new FilePickerService();
		var deviceDiscoveryService = new DeviceDiscoveryService(networkService, deviceInfoService);
		var fileTransferService = new FileTransferService();

		var viewModel = new MainPageViewModel(
			deviceDiscoveryService,
			fileTransferService,
			filePickerService,
			networkService);

		return new Window(new NavigationPage(new MainPage(viewModel)));
	}
}