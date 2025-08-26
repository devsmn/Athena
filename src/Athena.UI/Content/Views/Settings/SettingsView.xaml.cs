using Athena.Content.Views;

namespace Athena.UI;

public partial class SettingsView : ContentPage
{
    public SettingsView()
    {
        BindingContext = new SettingsViewModel();
        InitializeComponent();
        versionLabel.Text = $"{AppInfo.Current.Name} {AppInfo.Current.VersionString}-{AppInfo.Current.BuildString}";
    }
}
