using Athena.DataModel.Core;
using Athena.UI;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using Syncfusion.Maui.Core.Hosting;
using TesseractOcrMaui;

namespace Athena.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

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

#if DEBUG
        AdConfig.UseTestAdUnitIds = true;
        AdConfig.AddTestDevice("B3EEABB8EE11C2BE770B684D95219ECB");
#else
        AdConfig.UseTestAdUnitIds = false;
        AdConfig.DefaultInterstitialAdUnitId = "ca-app-pub-7134624676592827/8601607180";
        AdConfig.DefaultBannerAdUnitId = "ca-app-pub-7134624676592827/4690515785";
        AdConfig.DisableConsentCheck = true;
        //AdConfig.AddTestDevice("BA3156A459BD267FCF0E7B5173A25A2C");
#endif

        builder.Services.AddSingleton<IDataBrokerService, DefaultDataBrokerService>();
        builder.Services.AddSingleton<INavigationService, DefaultNavigationService>();
        builder.Services.AddSingleton<IPreferencesService, DefaultPreferencesService>();
        builder.Services.AddSingleton<IGreetingService, DefaultGreetingService>();
        builder.Services.AddSingleton<ILanguageService, DefaultLanguageService>();
        builder.Services.AddTransient<IPdfCreatorService, DefaultPdfCreatorService>();
        builder.Services.AddSingleton<ICompressionService, DefaultCompressionService>();
        builder.Services.AddSingleton<ICompatibilityService, DefaultCompatibilityService>();
        builder.Services.AddSingleton<IOcrService, DefaultOcrService>();
        builder.Services.AddTransient<IDownloadService, DefaultDownloadService>();
        builder.Services.AddSingleton<INetworkService, DefaultNetworkService>();

        var app = builder.Build();

        Services.Register(app.Services);

        return app;
    }
}
