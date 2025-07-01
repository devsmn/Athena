namespace Athena.UI;

public partial class PinView : ContentPage
{
	public PinView(TaskCompletionSource<string> tcs, bool newPassword = false)
    {
        BindingContext = new PinViewModel(tcs, newPassword);
		InitializeComponent();
	}
}
