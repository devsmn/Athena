using Athena.DataModel.Core;

namespace Athena.UI
{
    public class WelcomeViewLastStep : IViewStep<WelcomeViewModel>
    {
        public async Task ExecuteAsync(IContext context, WelcomeViewModel vm)
        {
            vm.CompatService.UpdateLastUsedVersion();
            vm.PrefService.SetFirstUsage();
            vm.PrefService.SetName(vm.Name);
            vm.LanguageService.SetLanguage(context, vm.SelectedLanguage.Id, true);
            vm.PrefService.SetLastTermsOfUseVersion(IPreferencesService.ToSVersion);
        }
    }
}
