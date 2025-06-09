using Athena.DataModel.Core;
using Athena.DataModel.Core.Platforms.Android;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using Syncfusion.Maui.Core.Hosting;

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
        AdConfig.DisableConsentCheck = true;
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
        builder.Services.AddSingleton<ISecureStorageService, DefaultSecureStorageService>();
        builder.Services.AddSingleton<IBiometricKeyService, AndroidBiometricKeyService>();
        builder.Services.AddSingleton<IDataEncryptionService, DefaultDataEncryptionService>();

        var app = builder.Build();
        Services.Register(app.Services);

        return app;
    }
}
