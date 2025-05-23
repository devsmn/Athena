namespace Athena.UI;

public partial class OcrLanguageSettings : DefaultContentPage
{
    public OcrLanguageSettings()
    {
        BindingContext = new OcrLanguageSettingsViewModel();
        InitializeComponent();
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

}
