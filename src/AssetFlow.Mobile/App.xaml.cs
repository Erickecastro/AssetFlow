namespace AssetFlow.Mobile;

public partial class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_mainPage)
        {
            Title = "AssetFlow Experimental"
        };

#if WINDOWS
        window.HandlerChanged += (_, _) => ConfigureWindowsTitleBar(window);
#endif

        return window;
    }

#if WINDOWS
    private static void ConfigureWindowsTitleBar(Window window)
    {
        if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
        {
            return;
        }

        try
        {
            var titleBar = nativeWindow.AppWindow.TitleBar;
            titleBar.BackgroundColor = Microsoft.UI.Colors.Black;
            titleBar.ForegroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Black;
            titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Black;
            titleBar.ButtonPressedBackgroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Black;
            titleBar.InactiveBackgroundColor = Microsoft.UI.Colors.Black;
            titleBar.InactiveForegroundColor = Microsoft.UI.Colors.White;
        }
        catch
        {
            // Alguns ambientes Windows não disponibilizam AppWindow.TitleBar durante a inicialização.
            // A aplicação continua com a barra nativa em vez de falhar ao abrir.
        }
    }
#endif
}
