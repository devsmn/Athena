using Athena.DataModel.Core;

namespace Athena.UI
{
    public class WelcomeViewLastStep : IViewStep<WelcomeViewModel>
    {
        public async Task ExecuteAsync(IContext context, WelcomeViewModel vm)
        {
            vm.PrefService.SetFirstUsage();
            vm.PrefService.SetName(vm.Name);
            vm.PrefService.SetLastTermsOfUseVersion(IPreferencesService.ToSVersion);
            vm.LanguageService.SetLanguage(context, vm.SelectedLanguage.Id, true);
        }
    }
}
