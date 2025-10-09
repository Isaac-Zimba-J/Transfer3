using Transfer3.ViewModels;

namespace Transfer3;

public partial class MainPage : ContentPage
{
	private readonly MainPageViewModel _viewModel;

	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Initialize the view model when page appears
		await _viewModel.InitializeCommand.ExecuteAsync(null);
	}

	protected override async void OnDisappearing()
	{
		base.OnDisappearing();

		// Stop discovery when leaving the page to save resources
		if (_viewModel.IsDiscovering)
		{
			await _viewModel.StopDiscoveryCommand.ExecuteAsync(null);
		}
	}

}

