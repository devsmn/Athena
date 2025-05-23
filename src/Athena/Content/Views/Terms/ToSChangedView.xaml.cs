using Athena.Content.Views;

namespace Athena.UI;

public partial class ToSChangedView : DefaultContentPage
{
	public ToSChangedView()
	{
        BindingContext = new ToSChangedViewModel();
        InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/privacy/"));
    }

    private async void OnTermsOfUseClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new WebViewPage("https://devsmn.github.io/Athena-Public/tos/"));
    }
}
