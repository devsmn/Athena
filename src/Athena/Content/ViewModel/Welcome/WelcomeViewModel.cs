using System.Collections.ObjectModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class WelcomeViewModel : ContextViewModel
    {
        [ObservableProperty]
        private ObservableCollection<LanguageViewModel> _languages;

        [ObservableProperty]
        private LanguageViewModel _selectedLanguage;

        [ObservableProperty]
        private int _step;

        [ObservableProperty]
        private string _greeting;

        [ObservableProperty]
        private string _enterNameText;

        [ObservableProperty]
        private string _enterNameTextDesc;

        [ObservableProperty]
        private string _namePlaceholder;

        [ObservableProperty]
        private string _welcomeTitle;

        [ObservableProperty]
        private string _chooseLanguageText;

        [ObservableProperty]
        private string _nextButtonText;

        [ObservableProperty]
        private string _welcomeReadyText;

        [ObservableProperty]
        private string _welcomeReadyTextDesc;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isConfirmChecked;

        public ILanguageService LanguageService { get; private set; }
        public IGreetingService GreetingService { get; private set; }
        public IPreferencesService PrefService { get; private set; }
        public ICompatibilityService CompatService { get; private set; }

        private readonly ViewStepHandler<WelcomeViewModel> _stepHandler;

        public ViewStepHandler<WelcomeViewModel> StepHandler => _stepHandler;

        public WelcomeViewModel()
        {
            _stepHandler = new(this);
            _stepHandler.RegisterIncrease(0);
            _stepHandler.RegisterIncrease(1);
            _stepHandler.RegisterIncrease(2);
            _stepHandler.RegisterIncrease(3);
            _stepHandler.Register(4, new WelcomeViewLastStep());

            GreetingService = Services.GetService<IGreetingService>();
            LanguageService = Services.GetService<ILanguageService>();
            PrefService = Services.GetService<IPreferencesService>();
            CompatService = Services.GetService<ICompatibilityService>();

            Languages = new ObservableCollection<LanguageViewModel>(LanguageService.GetSupportedLanguages());
            SelectedLanguage = Languages[0];

            SelectionChanged();
        }

        partial void OnIsConfirmCheckedChanged(bool value)
        {
            NextStepCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteNextStep()
        {
            return StepHandler.StepIndex != 2 || IsConfirmChecked;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteNextStep))]
        private async Task NextStep()
        {
            await StepHandler.Next(RetrieveContext());
        }

        public async Task BackButton()
        {
            await StepHandler.Back(RetrieveContext());
        }

        [RelayCommand]
        public void SelectionChanged()
        {
            LanguageService.SetLanguage(RetrieveContext(), SelectedLanguage.Id, false);

            EnterNameText = Localization.WelcomeEnterName;
            EnterNameTextDesc = Localization.WelcomeEnterNameDesc;
            NamePlaceholder = Localization.WelcomeYourNamePlaceholder;
            WelcomeTitle = Localization.WelcomeTitle;
            ChooseLanguageText = Localization.WelcomeSelectLanguage;
            NextButtonText = Localization.Next;
            WelcomeReadyTextDesc = Localization.WelcomeReadyDesc;
            WelcomeReadyText = Localization.WelcomeReady;
            Greeting = GreetingService.Get();
        }
    }
}
