namespace Athena.Content.Views;

public partial class WebViewPage : ContentPage
{
	public WebViewPage(string address)
	{
		InitializeComponent();
        webView.Source = address;
	}

    private void ButtonBase_OnClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
}