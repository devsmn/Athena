using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using ImageSource = Microsoft.Maui.Controls.ImageSource;

namespace Athena.UI
{
    public partial class SettingsItem : ObservableObject
    {
        [ObservableProperty]
        private ImageSource _imageSource;

        private readonly Func<Task> _createPage;

        [ObservableProperty]
        private LocalizedString _title;

        [ObservableProperty]
        private LocalizedString _description;

        public SettingsItem(string titleKey, string descKey, string icon, Func<Task> createPage)
        {
            Title = new(() => Localization.ResourceManager.GetString(titleKey));
            Description = new(() => Localization.ResourceManager.GetString(descKey));
            ImageSource = ImageSource.FromFile(icon);
            _createPage = createPage;
        }

        public async Task Clicked()
        {
            await _createPage();
        }
    }
}
