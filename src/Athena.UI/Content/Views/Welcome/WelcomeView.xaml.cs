namespace Athena.UI;

public partial class WelcomeView : DefaultContentPage
{
    private readonly WelcomeViewModel _vm;

    public WelcomeView()
    {
        _vm = new WelcomeViewModel(DoneTcs);
        BindingContext = _vm;
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        _vm.BackButton();
        return true;
    }
}
