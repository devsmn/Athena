using System.Globalization;
#if ANDROID
using AndroidX.AppCompat.App;
#endif
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Microsoft.Maui.Platform;

namespace Athena.UI
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "MzI4ODI2NkAzMjM1MmUzMDJlMzBvKzEwYS8vR29LZzhMMFUyOWd5bndnWi9NQ1FUY2FGcS9HNmlmTHNNZjVFPQ==");

            Application.Current.UserAppTheme = AppTheme.Light;
#if ANDROID
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
#endif
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };

            string lan = Preferences.Default.Get("Language", string.Empty);

            if (string.IsNullOrEmpty(lan))
            {
                lan = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                Preferences.Default.Set("Language", lan);
            }

            SettingsViewModel.SetLanguage(lan, new AthenaAppContext(), false);

            InitializeComponent();

            Task.Run(async () =>
            {
                SQLiteProxyParameter parameter = new SQLiteProxyParameter
                {
                    MinimumVersion = new Version(0, 1)
                };

                ServiceProvider.GetService<IDataBrokerService>().PrepareForLoading();

                DataStore.Register(SQLiteProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<IPageRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<ITagRepository>(parameter));

                await DataStore.InitializeAsync();
                
            }).ContinueWith(
                _ =>
                {
                    var context = new AthenaAppContext();
                    var folders = Folder.ReadAll(new AthenaAppContext());
                    ServiceProvider.GetService<IDataBrokerService>().Publish(context, folders, UpdateType.Initialize);
                });

            //MainPage = new NavigationPage(new FolderOverview());
            MainPage = new ContainerPage();

        }
    }
}