using Android.Graphics.Drawables;

namespace Athena.UI;

public partial class ToSChangedView : DefaultContentPage
{
    public const string Route = "tos";

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
