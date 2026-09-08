using AssetFlow.Mobile.ViewModels;
using AssetFlow.Mobile.Models;
using System.ComponentModel;

namespace AssetFlow.Mobile;

public partial class MainPage : ContentPage
{
    private CancellationTokenSource? _connectionBannerCancellation;
    private string _currentDestination = "dashboard";

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnNewAssetClicked(object? sender, EventArgs e)
    {
        await NavigateFromMenuAsync("assets");
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsAuthenticated))
        {
            MainThread.BeginInvokeOnMainThread(AnimateAuthenticationStateAsync);
        }
    }

    private async void AnimateAuthenticationStateAsync()
    {
        if (BindingContext is not MainViewModel viewModel) return;

        if (viewModel.IsAuthenticated)
        {
            AuthenticatedShell.Opacity = 0;
            AuthenticatedShell.TranslationX = 26;
            await Task.WhenAll(
                AuthenticatedShell.FadeToAsync(1, 320, Easing.CubicOut),
                AuthenticatedShell.TranslateToAsync(0, 0, 320, Easing.CubicOut));
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        UpdateConnectionBanner(Connectivity.Current.NetworkAccess, showRestoredMessage: false);
    }

    protected override void OnDisappearing()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        _connectionBannerCancellation?.Cancel();
        base.OnDisappearing();
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => UpdateConnectionBanner(e.NetworkAccess, showRestoredMessage: true));

    private async void UpdateConnectionBanner(NetworkAccess networkAccess, bool showRestoredMessage)
    {
        _connectionBannerCancellation?.Cancel();
        _connectionBannerCancellation?.Dispose();
        _connectionBannerCancellation = new CancellationTokenSource();

        if (networkAccess != NetworkAccess.Internet)
        {
            ConnectionBannerIcon.Source = "connection_lost.png";
            ConnectionBannerText.Text = "Sem conexão com a internet.";
            await ShowConnectionBannerAsync(autoHide: false, _connectionBannerCancellation.Token);
            return;
        }

        if (!showRestoredMessage)
        {
            ConnectionBanner.IsVisible = false;
            return;
        }

        ConnectionBannerIcon.Source = "connection_restored.png";
        ConnectionBannerText.Text = "Conexão restabelecida.";
        await ShowConnectionBannerAsync(autoHide: true, _connectionBannerCancellation.Token);
    }

    private async Task ShowConnectionBannerAsync(bool autoHide, CancellationToken cancellationToken)
    {
        try
        {
            ConnectionBanner.Opacity = 0;
            ConnectionBanner.TranslationY = -20;
            ConnectionBanner.IsVisible = true;
            await Task.WhenAll(
                ConnectionBanner.FadeToAsync(1, 240, Easing.CubicOut),
                ConnectionBanner.TranslateToAsync(0, 0, 240, Easing.CubicOut));

            if (!autoHide)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
            await Task.WhenAll(
                ConnectionBanner.FadeToAsync(0, 220, Easing.CubicIn),
                ConnectionBanner.TranslateToAsync(0, -16, 220, Easing.CubicIn));
            ConnectionBanner.IsVisible = false;
        }
        catch (OperationCanceledException)
        {
            // A mudança seguinte de conectividade substitui esta mensagem.
        }
    }

    private async void OnProfileTapped(object? sender, TappedEventArgs e) =>
        await DisplayAlertAsync("Administrador", "Sessão autenticada no ambiente experimental do AssetFlow.", "Fechar");

    private async void OnNavigationTapped(object? sender, TappedEventArgs e)
    {
        var destination = e.Parameter as string ??
            (sender as TapGestureRecognizer)?.CommandParameter as string;
        if (!string.IsNullOrWhiteSpace(destination))
        {
            await NavigateFromMenuAsync(destination);
        }
    }

    private async void OnMobileNavigationClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string destination })
        {
            await NavigateFromMenuAsync(destination);
        }
    }

    private async Task NavigateFromMenuAsync(string destination)
    {
        if (string.Equals(_currentDestination, destination, StringComparison.Ordinal))
        {
            return;
        }

        await SetActiveNavigationAsync(destination);
        await NavigateToAsync(destination, animate: true);
    }

    private async Task SetActiveNavigationAsync(string destination)
    {
        var items = new (string Key, TapGestureRecognizer Gesture)[]
        {
            ("dashboard", DashboardNav),
            ("inventory", InventoryNav),
            ("assets", AssetsNav),
            ("departments", DepartmentsNav),
            ("movements", MovementsNav),
            ("reports", ReportsNav),
            ("users", UsersNav),
            ("settings", SettingsNav)
        };

        foreach (var (key, gesture) in items)
        {
            if (gesture.Parent is not Border border ||
                border.Content is not Grid grid ||
                grid.Children.OfType<Label>().FirstOrDefault() is not Label label)
            {
                continue;
            }

            var isActive = key == destination;
            border.BackgroundColor = isActive ? Color.FromArgb("#F1F3F5") : Colors.White;
            border.StrokeThickness = 0;
            label.TextColor = Color.FromArgb("#111827");
            label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;

        }

        await Task.CompletedTask;
    }

    private async Task NavigateToAsync(string destination, bool animate)
    {
        if (animate)
        {
            await AuthenticatedScroll.FadeToAsync(0, 100, Easing.CubicIn);
        }

        var showDashboard = destination == "dashboard";
        var showInventory = destination is "inventory" or "assets";
        var showAssets = destination == "assets";
        var showDepartments = destination == "departments";
        var showMovements = destination == "movements";

        StatsGrid.IsVisible = showDashboard;
        DashboardDivider.IsVisible = showDashboard;
        DashboardInsights.IsVisible = showDashboard;
        DashboardQuickActions.IsVisible = showDashboard;
        NewAssetAction.IsVisible = showAssets;
        FormsGrid.IsVisible = showAssets || showDepartments;
        AssetFormCard.IsVisible = showAssets;
        DepartmentFormCard.IsVisible = showDepartments;
        FormsDivider.IsVisible = showAssets || showDepartments;
        ListsGrid.IsVisible = showInventory || showDepartments;
        InventoryCard.IsVisible = showInventory;
        DepartmentsCard.IsVisible = showDepartments;
        DetailsCard.IsVisible = showMovements &&
            BindingContext is MainViewModel { HasSelectedAsset: true };

        (SectionTitle.Text, SectionSubtitle.Text) = destination switch
        {
            "inventory" => ("Inventário", "Consulte e pesquise todos os ativos cadastrados"),
            "assets" => ("Ativos", "Cadastre, edite ou exclua ativos do inventário"),
            "departments" => ("Departamentos", "Organize ativos por departamento e localização"),
            "movements" => ("Movimentações", "Acompanhe atribuições, transferências e devoluções"),
            "reports" => ("Relatórios", "Análises e exportações do inventário"),
            "users" => ("Usuários", "Gestão de acessos ao AssetFlow"),
            "settings" => ("Configurações", "Preferências e conexão com a API"),
            _ => ("Dashboard", "Visão geral do inventário de ativos")
        };

        await AuthenticatedScroll.ScrollToAsync(0, 0, false);
        _currentDestination = destination;

        if (animate)
        {
            AuthenticatedScroll.Opacity = 0;
            await AuthenticatedScroll.FadeToAsync(1, 200, Easing.CubicOut);
        }

        switch (destination)
        {
            case "movements" when DetailsCard.IsVisible:
                break;
            case "movements":
                await DisplayAlertAsync("Movimentações", "Selecione um ativo no inventário para consultar e registrar movimentações.", "Entendi");
                break;
            case "reports":
                await DisplayAlertAsync("Relatórios", "O módulo de relatórios ainda não possui contratos ou endpoints implementados.", "Fechar");
                break;
            case "users":
                await DisplayAlertAsync("Usuários", "A versão experimental utiliza o usuário administrador configurado na API.", "Fechar");
                break;
            case "settings":
                await ConfigureApiAddressAsync();
                break;
        }
    }

    private async void OnEditDepartmentClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: DepartmentModel department } && BindingContext is MainViewModel viewModel)
        {
            viewModel.BeginEditDepartment(department);
            if (_currentDestination != "departments")
            {
                await SetActiveNavigationAsync("departments");
                await NavigateToAsync("departments", animate: false);
            }
        }
    }

    private async void OnDeleteDepartmentClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: DepartmentModel department } || BindingContext is not MainViewModel viewModel) return;
        var confirmed = await DisplayAlertAsync("Excluir departamento", $"Excluir o departamento {department.Name}?", "Confirmar", "Cancelar");
        if (confirmed) await viewModel.DeleteDepartmentAsync(department);
    }

    private async void OnEditAssetClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: AssetModel asset } && BindingContext is MainViewModel viewModel)
        {
            viewModel.BeginEditAsset(asset);
            if (_currentDestination != "assets")
            {
                await SetActiveNavigationAsync("assets");
                await NavigateToAsync("assets", animate: false);
            }
        }
    }

    private async void OnDeleteAssetClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: AssetModel asset } || BindingContext is not MainViewModel viewModel) return;
        var confirmed = await DisplayAlertAsync("Excluir ativo", $"Excluir definitivamente o ativo {asset.Code}?", "Confirmar", "Cancelar");
        if (confirmed) await viewModel.DeleteAssetAsync(asset);
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync("Sair do AssetFlow", "Deseja realmente encerrar a sessão?", "Confirmar", "Cancelar");
        if (!confirmed || BindingContext is not MainViewModel viewModel) return;
        await Task.WhenAll(
            AuthenticatedShell.FadeToAsync(0, 220, Easing.CubicIn),
            AuthenticatedShell.TranslateToAsync(20, 0, 220, Easing.CubicIn));
        if (viewModel.LogoutCommand.CanExecute(null)) viewModel.LogoutCommand.Execute(null);
        AuthenticatedShell.TranslationX = 0;
        AuthenticatedShell.Opacity = 1;
    }

    private async Task ConfigureApiAddressAsync()
    {
        var fallback = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5014/"
            : "http://localhost:5014/";
        var current = Preferences.Default.Get("AssetFlow.ApiBaseAddress", fallback);
        var value = await DisplayPromptAsync(
            "Configurações",
            "Endereço completo da API. Em um telefone físico, use o endereço da rede local ou o endpoint HTTPS publicado.",
            "Salvar",
            "Cancelar",
            initialValue: current,
            keyboard: Keyboard.Url);

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var address) ||
            address.Scheme is not ("http" or "https"))
        {
            await DisplayAlertAsync("Endereço inválido", "Informe uma URL iniciada por http:// ou https://.", "Fechar");
            return;
        }

        var normalized = address.AbsoluteUri.EndsWith('/') ? address.AbsoluteUri : $"{address.AbsoluteUri}/";
        Preferences.Default.Set("AssetFlow.ApiBaseAddress", normalized);
        await DisplayAlertAsync("Configuração salva", "Reinicie o aplicativo para utilizar o novo endereço da API.", "Entendi");
    }
}
