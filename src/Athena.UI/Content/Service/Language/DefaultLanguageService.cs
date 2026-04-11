using System.Globalization;
using Athena.DataModel.Core;
using Athena.Resources.Localization;

namespace Athena.UI
{
    internal class DefaultLanguageService : ILanguageService
    {
        public async Task SetLanguage(IContext context, string code, bool reload)
        {
            try
            {
                IPreferencesService prefService = Services.GetService<IPreferencesService>();

                prefService.SetLanguage(code);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(code);
                Localization.Culture = CultureInfo.CurrentUICulture;

                if (!reload)
                    return;

                INavigationService navService = Services.GetService<INavigationService>();
                if (await navService.DisplayAlert("Restart required", "A restart is required to complete the changes. Close Athena now?", "Yes", "No"))
                {
                    Application.Current.Quit();
                }
                //Application.Current.Windows[0].Page = new ContainerPage();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        public IEnumerable<LanguageViewModel> GetSupportedLanguages()
        {
            return new List<LanguageViewModel> {
                new ("English", "en-US"),
                new ("Deutsch", "de-DE")
            };
        }

        public string GetLanguage()
        {
            return Services.GetService<IPreferencesService>().GetLanguage();
        }
    }
}
