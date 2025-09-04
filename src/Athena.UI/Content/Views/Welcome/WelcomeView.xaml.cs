namespace Athena.UI;

public partial class WelcomeView : ContentPage
{
    private readonly WelcomeViewModel _vm;

    public WelcomeView()
    {
        InitializeComponent();
        _vm = BindingContext as WelcomeViewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        _vm.BackButton();
        return true;
    }
}
