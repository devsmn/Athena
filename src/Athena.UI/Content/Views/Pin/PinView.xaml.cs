namespace Athena.UI;

public partial class PinView : ContentPage
{
    public const string Route = "password";


    public PinView(TaskCompletionSource<string> tcs)
    {
        BindingContext = new PasswordViewModel(tcs);
		InitializeComponent();
	}
}
