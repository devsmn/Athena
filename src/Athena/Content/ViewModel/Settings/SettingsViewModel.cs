using System.Collections.ObjectModel;
using System.Globalization;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;

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

        public SettingsViewModel()
        {
            Languages = new ObservableCollection<LanguageViewModel> {
                new LanguageViewModel("English (US)", "en-US"),
                new LanguageViewModel("German", "de-DE")
            };
            
            string lan = Preferences.Default.Get("Language", "en-US");
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

            Preferences.Default.Set("Language", SelectedLanguage.Id);

            SetLanguage(SelectedLanguage.Id, this.RetrieveContext(), true);

            await Toast.Make("Successfully changed the language").Show();
        }

        internal static void SetLanguage(string code, IContext context, bool reload)
        {
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(code);
                Localization.Culture = CultureInfo.CurrentUICulture;

                if (!reload)
                    return;
                Application.Current.MainPage = new ContainerPage();

                var folders = Folder.ReadAll(context);
                ServiceProvider.GetService<IDataBrokerService>().Publish(context, folders, UpdateType.Initialize);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        [RelayCommand]
        private async Task ShowTutorial()
        {
            await PushModalAsync(new TutorialView());
        }

        [RelayCommand]
        private void ShowNews()
        {
            NewsText = Localization.NewsText;
            IsNewsPopupOpen = true;
        }
    }

}
