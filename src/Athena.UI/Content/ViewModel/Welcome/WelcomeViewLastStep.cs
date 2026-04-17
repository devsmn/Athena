using Athena.DataModel.Core;

namespace Athena.UI
{
    public class WelcomeViewLastStep : IViewStep<WelcomeViewModel>
    {
        private readonly TaskCompletionSource _doneTcs;

        public WelcomeViewLastStep(TaskCompletionSource doneTcs)
        {
            _doneTcs = doneTcs;
        }

        public async Task ExecuteAsync(IContext context, WelcomeViewModel vm)
        {
            vm.PrefService.SetFirstUsage();
            vm.PrefService.SetName(vm.Name);
            vm.PrefService.SetLastTermsOfUseVersion(IPreferencesService.ToSVersion);
            vm.LanguageService.SetLanguage(context, vm.SelectedLanguage.Id);
            _doneTcs.SetResult();
        }
    }
}
