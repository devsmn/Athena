namespace Athena.UI;

public partial class PinView : ContentPage
{
	public PinView(TaskCompletionSource<string> tcs)
    {
        BindingContext = new PinViewModel(tcs);
		InitializeComponent();
	}
}
