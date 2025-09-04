using System.Globalization;

#if ANDROID
using AndroidX.AppCompat.App;
#endif

using Athena.DataModel.Core;
namespace Athena.UI
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                SecretConfig.SyncfusionLicenseKey);

            Current.UserAppTheme = AppTheme.Light;
#if ANDROID
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
#endif
            RequestedThemeChanged += (s, e) => { Current.UserAppTheme = AppTheme.Light; };

            string lan = Preferences.Default.Get("Language", string.Empty);

            if (string.IsNullOrEmpty(lan))
            {
                lan = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            }

            ILanguageService languageService = Services.GetService<ILanguageService>();
            languageService.SetLanguage(new AthenaAppContext(), lan, false);

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new ContainerPage());
        }
    }
}
