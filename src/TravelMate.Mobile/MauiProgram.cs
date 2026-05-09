using Microsoft.Extensions.Logging;
using TravelMate.Mobile.Services;

namespace TravelMate.Mobile;

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

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5068/"
                : "http://localhost:5068/")
        });
        builder.Services.AddSingleton<TravelMateApiClient>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
