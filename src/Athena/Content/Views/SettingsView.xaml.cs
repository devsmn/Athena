using Athena.Content.Views;

namespace Athena.UI;

public partial class SettingsView : ContentPage
{
    public SettingsView()
    {
        BindingContext = new SettingsViewModel();
        InitializeComponent();
        versionLabel.Text = $"{AppInfo.Current.Name} v{AppInfo.Current.VersionString}-{AppInfo.Current.BuildString}";
    }

    private async void OnChangelogClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/app_changelog/"));
    }

    private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/privacy/"));
    }

    private async void OnTermsOfUseClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/tos/"));
    }

    private async void OnCopyrightClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/copyright/"));
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/app_help/"));
    }

    private async void OnFeedbackClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://forms.gle/SDAERdx1JGny77EZ7"));
    }
}