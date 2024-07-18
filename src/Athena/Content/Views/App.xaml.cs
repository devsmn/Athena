using System.Globalization;
#if ANDROID
using AndroidX.AppCompat.App;
#endif
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Athena.DataModel.Core;
using Microsoft.Maui.Platform;
namespace Athena.UI
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "MzM4ODU5NUAzMjM2MmUzMDJlMzBOYkJYWWxSMGRMOHJDc1pucWxtMmNlT1VzSldCbEIyVThhcFRhLytmS1hjPQ==");

            Application.Current.UserAppTheme = AppTheme.Light;
#if ANDROID
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
#endif
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };

            string lan = Preferences.Default.Get("Language", string.Empty);

            if (string.IsNullOrEmpty(lan))
            {
                lan = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            }

            ILanguageService languageService = ServiceProvider.GetService<ILanguageService>();
            languageService.SetLanguage(new AthenaAppContext(), lan, false);

            InitializeComponent();

            Task.Run(async () =>
            {
                SqLiteProxyParameter parameter = new SqLiteProxyParameter
                {
                    MinimumVersion = new Version(0, 1)
                };

                ServiceProvider.GetService<IDataBrokerService>().PrepareForLoading();

                DataStore.Register(SqLiteProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<ITagRepository>(parameter));
                await DataStore.InitializeAsync();
            }).ContinueWith(
                _ =>
                {
                    InitializeData();
                });

            MainPage = new ContainerPage();

            Application.Current.ModalPopped += CurrentOnModalPopped;
        }

        public static void InitializeData()
        {
            // Data will be initialized via the welcome view.
            if (ServiceProvider.GetService<IPreferencesService>().IsFirstUsage())
                return;

            IDataBrokerService service = ServiceProvider.GetService<IDataBrokerService>();
            var context = new AthenaAppContext();

            Folder rootFolder = GetRootFolder(context);

            service.SetRootFolder(rootFolder);
            service.Publish(context, rootFolder.Folders, UpdateType.Initialize);
            service.Publish(context, rootFolder.Documents, UpdateType.Initialize);
            service.RaiseAppInitialized();
        }

        private static Folder GetRootFolder(IContext context)
        {
            var rootFolder = Folder.Read(context, IntegerEntityKey.Root);

            if (rootFolder != null)
                return rootFolder;

            Folder.CreateRoot(context);
            return Folder.Read(context, IntegerEntityKey.Root);
        }

        private void CurrentOnModalPopped(object sender, ModalPoppedEventArgs e)
        {
            if (e.Modal?.BindingContext is not ContextViewModel vm)
                return;

            vm.Dispose();
        }
    }
}