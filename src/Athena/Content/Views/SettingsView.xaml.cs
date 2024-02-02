using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace Athena.UI;

public partial class SettingsView : ContentPage
{
	public SettingsView()
	{

        this.BindingContext = new SettingsViewModel();
        InitializeComponent();
        versionLabel.Text = $"{AppInfo.Current.Name} v{AppInfo.Current.VersionString}-{AppInfo.Current.BuildString}";
    }

    private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
    {
        await Launcher.TryOpenAsync(
            "https://doc-hosting.flycricket.io/athena-ai-document-manager-privacy-policy/735ec806-6f6f-46aa-b4f3-223e208a9a81/privacy");
    }

    private async void OnTermsOfUseClicked(object sender, EventArgs e)
    {
        await Launcher.TryOpenAsync(
            "https://doc-hosting.flycricket.io/athena-ai-document-manager-terms-of-use/d052c3bc-5584-4215-a719-fd6ac0dff5e2/terms");
    }

    private async void OnCopyrightClicked(object sender, EventArgs e)
    {
        await Launcher.TryOpenAsync(
            "https://doc-hosting.flycricket.io/copyright/189371f8-6a46-4600-9e78-21a72a7e767e/other");
    }
}