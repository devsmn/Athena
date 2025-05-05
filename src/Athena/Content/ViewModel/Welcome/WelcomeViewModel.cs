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

        private readonly ILanguageService _languageService;
        private readonly IGreetingService _greetingService;
        private readonly IPreferencesService _prefService;
        private readonly ICompatibilityService _compatService;

        public WelcomeViewModel()
        {
            _greetingService = Services.GetService<IGreetingService>();
            _languageService = Services.GetService<ILanguageService>();
            _prefService = Services.GetService<IPreferencesService>();
            _compatService = Services.GetService<ICompatibilityService>();

            Languages = new ObservableCollection<LanguageViewModel>(_languageService.GetSupportedLanguages());
            SelectedLanguage = Languages[0];

            SelectionChanged();
        }

        [RelayCommand]
        private async Task NextStep()
        {
            Step++;

            if (Step < 2)
            {
                NextButtonText = Localization.Next;
            }

            if (Step > 2)
            {
                _compatService.UpdateLastUsedVersion();
                _prefService.SetFirstUsage();
                _prefService.SetName(Name);
                _languageService.SetLanguage(RetrieveContext(), SelectedLanguage.Id, true);
            }
            else if (Step > 1)
            {
                NextButtonText = Localization.Close;
            }
        }

        public void BackButton()
        {
            Step = Math.Max(--Step, 0);
        }

        [RelayCommand]
        public void SelectionChanged()
        {
            _languageService.SetLanguage(RetrieveContext(), SelectedLanguage.Id, false);

            EnterNameText = Localization.WelcomeEnterName;
            EnterNameTextDesc = Localization.WelcomeEnterNameDesc;
            NamePlaceholder = Localization.WelcomeYourNamePlaceholder;
            WelcomeTitle = Localization.WelcomeTitle;
            ChooseLanguageText = Localization.WelcomeSelectLanguage;
            NextButtonText = Localization.Next;
            WelcomeReadyTextDesc = Localization.WelcomeReadyDesc;
            WelcomeReadyText = Localization.WelcomeReady;
            Greeting = _greetingService.Get();
        }
    }
}
