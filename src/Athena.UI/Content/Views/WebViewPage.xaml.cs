namespace Athena.Content.Views;

public partial class WebViewPage : ContentPage
{
    private readonly string _address;

    public WebViewPage(string address)
    {
        InitializeComponent();
        webView.Source = address;
        _address = address;
    }

    private void CloseClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private async void OpenInBrowserClicked(object sender, EventArgs e)
    {
        await Launcher.TryOpenAsync(_address);
    }
}