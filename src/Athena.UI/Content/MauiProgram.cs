using Athena.DataModel.Core;
using Athena.DataModel.Core.Platforms.Android;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using Serilog;
using Syncfusion.Maui.Core.Hosting;

namespace Athena.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();

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
#endif

        string extDir = Android.App.Application.Context.GetExternalFilesDir(null)?.AbsolutePath;

        if (string.IsNullOrEmpty(extDir))
        {
            extDir = $@"../{FileSystem.CacheDirectory}";
            extDir += "/files/";
        }

        string logFolder = Path.Combine(extDir, "logs");
        Directory.CreateDirectory(logFolder);

        builder.Services.AddSerilog(
            new LoggerConfiguration()
                .WriteTo
                    .File(
                        Path.Combine(logFolder, "log_.txt"),
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 20000000, // Cap at 20mb
                        rollOnFileSizeLimit: true,
                        shared: true,
                        retainedFileCountLimit: 14,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message}{NewLine}{Exception}")
#if DEBUG
                .WriteTo
                    .Debug(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message}{NewLine}{Exception}")
#endif
                .CreateLogger());

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
        builder.Services.AddSingleton<IHardwareKeyStoreService, AndroidHardwareKeyStoreService>();
        builder.Services.AddSingleton<IDataEncryptionService, DefaultDataEncryptionService>();
        builder.Services.AddSingleton<IPasswordService, DefaultPasswordService>();
        builder.Services.AddSingleton<IDocumentScannerService>(DefaultDocumentScannerService.Instance);
        builder.Services.AddSingleton<IBackupService, DefaultBackupService>();

        MauiApp app = builder.Build();
        Services.Register(app.Services);

        return app;
    }
}
