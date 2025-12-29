using Athena.Resources.Localization;

namespace Athena.UI;

public partial class BackupSettingsView : DefaultContentPage
{
    public const string Route = "settings/backup";


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
