using System.Collections.ObjectModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    internal partial class SettingsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private ObservableCollection<LanguageViewModel> _languages;

        [ObservableProperty]
        private LanguageViewModel _selectedLanguage;

        [ObservableProperty]
        private bool _isNewsPopupOpen;

        [ObservableProperty]
        private string _newsText;

        private readonly ILanguageService _languageService;

        public SettingsViewModel()
        {
            _languageService = Services.GetService<ILanguageService>();
            Languages = new ObservableCollection<LanguageViewModel>(_languageService.GetSupportedLanguages());

            string lan = _languageService.GetLanguage();
            var vm = Languages.FirstOrDefault(x => x.Id == lan);
            SelectedLanguage = vm ?? Languages[0];
        }

        [RelayCommand]
        private async Task SelectionChanged()
        {
            bool confirm = await DisplayAlert(
                Localization.Language,
                string.Format(Localization.ChangeLanguageConfirm, SelectedLanguage.Name),
                Localization.Yes,
                Localization.No);

            if (!confirm)
            {
                SelectedLanguage = SelectedLanguage == Languages[0] ? Languages[1] : Languages[0];
                return;
            }

            _languageService.SetLanguage(RetrieveContext(), SelectedLanguage.Id, true);

            await Toast.Make("Successfully changed the language").Show();
        }
    }

}
