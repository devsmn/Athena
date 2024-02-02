using CommunityToolkit.Maui;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using TesseractOcrMaui;
using Syncfusion.Maui.Core.Hosting;
using FFImageLoading.Maui;

namespace Athena.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if DEBUG
        AdConfig.UseTestAdUnitIds = true;
        AdConfig.AddTestDevice("BA3156A459BD267FCF0E7B5173A25A2C");
#else
        //AdConfig.DefaultInterstitialAdUnitId = "ca-app-pub-7134624676592827/8601607180";
        AdConfig.DefaultInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        AdConfig.DefaultBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        AdConfig.AddTestDevice("BA3156A459BD267FCF0E7B5173A25A2C");
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseAdMob()
            .UseFFImageLoading()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        //builder.Logging.AddDebug();
#endif
        
        builder.Services.AddTesseractOcr(
            files =>
            {
                files.AddFile("eng.traineddata");
                files.AddFile("deu.traineddata");
            });

        builder.Services.AddSingleton<IDataBrokerService, DataRequestService>();
        builder.Services.AddSingleton<INavigationService, DefaultNavigationService>();


        var app = builder.Build();

        ServiceProvider.Register(app.Services);

        return app;
    }
}
