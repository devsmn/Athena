using System.Globalization;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using LocalizationResourceManager.Maui;

namespace Athena.UI
{
    internal class DefaultLanguageService : ILanguageService
    {
        public async Task SetLanguage(IContext context, string code, bool reload)
        {
            try
            {
                ILocalizationResourceManager manager = Services.GetService<ILocalizationResourceManager>();
                IPreferencesService prefService = Services.GetService<IPreferencesService>();

                prefService.SetLanguage(code);
                CultureInfo info = CultureInfo.GetCultureInfoByIetfLanguageTag(code);
                Localization.Culture = info;
                manager.CurrentCulture = info;
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
