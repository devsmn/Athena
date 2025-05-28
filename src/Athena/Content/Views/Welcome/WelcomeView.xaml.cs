namespace Athena.UI;

public partial class WelcomeView : ContentPage
{
    public WelcomeView()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as WelcomeViewModel).BackButton();
        return true;
    }
}
