using System.Collections.ObjectModel;
using Athena.Content.Views;
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

        [ObservableProperty]
        private List<SettingsItem> _settingsItems;

        [ObservableProperty]
        private SettingsItem _selectedSetting;

        public SettingsViewModel()
        {
            _languageService = Services.GetService<ILanguageService>();
            Languages = new ObservableCollection<LanguageViewModel>(_languageService.GetSupportedLanguages());

            string lan = _languageService.GetLanguage();
            LanguageViewModel vm = Languages.FirstOrDefault(x => x.Id == lan);
            SelectedLanguage = vm ?? Languages[0];

            SettingsItems = new List<SettingsItem>
            {
                new(
                    Localization.SettingsPrivacyPolicyTitle,
                    Localization.SettingsPrivacyPolicyDesc,
                    "privacy.png",
                    () => OpenSettings<WebViewPage>("https://athena.devsmn.de/tos/")),

                new(
                    Localization.SettingsTermsOfUseTitle,
                    Localization.SettingsTermsOfUseDesc,
                    "terms_and_conditions.png",
                    () => OpenSettings<WebViewPage>("https://athena.devsmn.de/privacy/")),

                new(
                    Localization.SettingsCopyrightTitle,
                    Localization.SettingsCopyrightDesc,
                    "copyright.png",
                    () => OpenSettings<WebViewPage>("https://athena.devsmn.de/copyright/")),

                new(
                    Localization.SettingsNewsDesc,
                    Localization.SettingsNewsTitle,
                    "news.png",
                    () => OpenSettings<WebViewPage>("https://athena.devsmn.de/app_changelog/")),

                new(
                    Localization.SettingsHelpTitle,
                    Localization.SettingsHelpDesc,
                    "help.png",
                    () => OpenSettings<WebViewPage>("https://athena.devsmn.de/app_help/")),

                new(
                    Localization.SettingsFeedbackTitle,
                    Localization.SettingsFeedbackDesc,
                    "feedback.png",
                    () => OpenSettings<WebViewPage>("https://forms.gle/SDAERdx1JGny77EZ7")),

                new(
                    Localization.SettingsOcrTitle,
                    Localization.SettingsOcrDesc,
                    "ocr.png",
                    () => OpenSettings<OcrLanguageSettings>()),

                new(
                    Localization.DocumentScanner,
                    Localization.SettingsDocumentScannerDesc,
                    "scan.png",
                    () => OpenSettings<DocumentScannerTypeSettingsView>()),

                new(
                    Localization.SettingsBackupTitle,
                    Localization.SettingsBackupDesc,
                    "backup.png",
                    () => OpenSettings<BackupSettingsView>()),

                new(
                    Localization.SettingsSecurityTitle,
                    Localization.SettingsSecurityDesc,
                    "security.png",
                    () => OpenSettings<SecuritySettingsView>()),
            };
        }

        [RelayCommand]
        private async Task ItemClicked()
        {
            if (SelectedSetting == null)
                return;

            await SelectedSetting.Clicked();
            SelectedSetting = null;
        }

        private async Task OpenSettings<TSettings>(params object[] args)
            where TSettings : Page
        {
            TSettings page;

            if (args == null)
                page = Activator.CreateInstance<TSettings>();
            else
                page = Activator.CreateInstance(typeof(TSettings), args, null) as TSettings;

            await PushModalAsync(page);
        }

        [RelayCommand]
        private async Task SelectionChanged()
        {
            bool confirm = await DisplayAlert(
                Localization.DisplayLanguage,
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
