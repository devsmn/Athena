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
}
