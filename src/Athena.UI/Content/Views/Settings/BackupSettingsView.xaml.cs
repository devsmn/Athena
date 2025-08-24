namespace Athena.UI;

public partial class BackupSettingsView : ContentPage
{
	public BackupSettingsView()
    {
        BindingContext = new BackupSettingsViewModel();
		InitializeComponent();
	}
}
