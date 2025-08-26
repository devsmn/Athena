namespace Athena.UI;

public partial class SecuritySettingsView : ContentPage
{
	public SecuritySettingsView()
	{
        BindingContext = new SecuritySettingsViewModel();
		InitializeComponent();
	}
}
