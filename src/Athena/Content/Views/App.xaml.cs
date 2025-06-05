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
                "MzgyMTA3MUAzMjM5MmUzMDJlMzAzYjMyMzkzYlFZSU9ScDVmeUJ5a1JHT0puWittU0VBWnE3S2tXblNMVzlDRy9kVitWWTA9");

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
            Current.ModalPopped += CurrentOnModalPopped;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new ContainerPage());
        }

        private void CurrentOnModalPopped(object sender, ModalPoppedEventArgs e)
        {
            if (e.Modal?.BindingContext is not ContextViewModel vm)
                return;

            vm.Dispose();
        }
    }
}
