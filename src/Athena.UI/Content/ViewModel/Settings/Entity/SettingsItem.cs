using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageSource = Microsoft.Maui.Controls.ImageSource;

namespace Athena.UI
{
    public partial class SettingsItem : ObservableObject
    {
        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private ImageSource _imageSource;

        private readonly Func<Task> _createPage;

        public SettingsItem(string title, string desc, string icon, Func<Task> createPage)
        {
            Title = title;
            Description = desc;
            ImageSource = ImageSource.FromFile(icon);
            _createPage = createPage;
        }

        public async Task Clicked()
        {
            await _createPage();
        }
    }
}
