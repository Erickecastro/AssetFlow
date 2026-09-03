using AssetFlow.Mobile.ViewModels;

namespace AssetFlow.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnNewAssetClicked(object? sender, EventArgs e)
    {
        await AuthenticatedScroll.ScrollToAsync(AssetFormCard, ScrollToPosition.Start, true);
    }
}
