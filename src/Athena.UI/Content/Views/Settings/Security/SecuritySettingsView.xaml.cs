namespace Athena.UI;

public partial class SecuritySettingsView : ContentPage
{
    public const string Route = "settings/security";

    public SecuritySettingsView()
	{
        BindingContext = new SecuritySettingsViewModel();
		InitializeComponent();
	}
}
