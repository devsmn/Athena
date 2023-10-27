using Athena.Data.SQLite.Provider;
using Athena.DataModel;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Athena.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddSingleton<IDataBrokerService, DataRequestService>();

        var app = builder.Build();

        ServiceProvider.Register(app.Services);

        return app;
    }
}
