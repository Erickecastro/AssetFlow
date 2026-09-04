using AssetFlow.Mobile.Services;
using AssetFlow.Mobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var defaultApiAddress = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5014/"
            : "http://localhost:5014/";
        var apiAddress = Preferences.Default.Get("AssetFlow.ApiBaseAddress", defaultApiAddress);
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(apiAddress),
            Timeout = TimeSpan.FromSeconds(15)
        });
        builder.Services.AddSingleton<AssetFlowApiClient>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
