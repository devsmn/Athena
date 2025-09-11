using Athena.Resources.Localization;

namespace Athena.UI;

public partial class BackupSettingsView : DefaultContentPage
{
	public BackupSettingsView()
    {
        BindingContext = new BackupSettingsViewModel();
		InitializeComponent();
	}

    private void OnInfoClicked(object sender, EventArgs e)
    {
        ShowInfoPopup(Localization.BackupTitle, Localization.BackupDesc);
    }
}
