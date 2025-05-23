namespace Athena.UI;

public partial class DefaultContentPage : ContentPage
{
    private bool _initialized;

	public DefaultContentPage()
	{
		InitializeComponent();

        NavigatedTo += OnNavigatedTo;
    }

    private async void OnNavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (!_initialized && BindingContext is ContextViewModel vm)
            await vm.InitializeAsync();

        _initialized = true;
    }
}
