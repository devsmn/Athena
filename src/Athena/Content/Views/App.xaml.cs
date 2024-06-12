using System.Globalization;
#if ANDROID
using AndroidX.AppCompat.App;
#endif
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Microsoft.Maui.Platform;
using Page = Athena.DataModel.Page;

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
                SqLiteProxyParameter parameter = new SqLiteProxyParameter
                {
                    MinimumVersion = new Version(0, 1)
                };

                ServiceProvider.GetService<IDataBrokerService>().PrepareForLoading();

                DataStore.Register(SqLiteProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IPageRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<ITagRepository>(parameter));

                await DataStore.InitializeAsync();

            }).ContinueWith(
                _ => {
                    AddDefaultData();

                    var context = new AthenaAppContext();
                    var folders = Folder.ReadAll(new AthenaAppContext());
                    ServiceProvider.GetService<IDataBrokerService>().Publish(context, folders, UpdateType.Initialize);
                });
            
            MainPage = new ContainerPage();

            Application.Current.ModalPopped += CurrentOnModalPopped;
        }

        private void CurrentOnModalPopped(object sender, ModalPoppedEventArgs e)
        {
            if (e.Modal?.BindingContext is not ContextViewModel vm)
                return;

            vm.Dispose();
        }

        private void AddDefaultData()
        {
            if (!ServiceProvider.GetService<IPreferencesService>().IsFirstUsage())
                return;

            Folder dummyFolder = new Folder
            {
                Name = "Insurance",
                Comment = "(Example folder) John"
            };

            Page dummyPage = new Page
            {
                Title = "Truck",
                Comment = "(Example page) Ford F150"
            };

            dummyFolder.AddPage(dummyPage);
            dummyFolder.Save(new AthenaAppContext());
        }
    }
}