using Athena.Content.Views;

namespace Athena.UI;

public partial class ToSAcceptView : ContentView
{
    public ToSAcceptView()
    {
        InitializeComponent();
    }

    private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://athena.devsmn.de/privacy/"));
    }

    private async void OnTermsOfUseClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://athena.devsmn.de/tos/"));
    }
}
