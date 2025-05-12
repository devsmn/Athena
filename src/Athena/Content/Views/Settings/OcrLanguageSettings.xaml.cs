namespace Athena.UI;

public partial class OcrLanguageSettings : ContentPage
{
    public OcrLanguageSettings()
    {
        BindingContext = new OcrLanguageSettingsViewModel();
        InitializeComponent();

        NavigatedTo += OnNavigatedTo;

    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () =>
        {
            bool canClose = await (BindingContext as OcrLanguageSettingsViewModel).CanClose();

            if (canClose)
                await Navigation.PopModalAsync();
        });

        return true;
    }

    private async void OnNavigatedTo(object sender, NavigatedToEventArgs e)
    {
        await (BindingContext as OcrLanguageSettingsViewModel).LoadAllSupportedLanguagesAsync();
    }

}
