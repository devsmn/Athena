using Athena.Resources.Localization;

namespace Athena.UI;

public partial class DocumentScannerTypeSettingsView : DefaultContentPage
{
    public DocumentScannerTypeSettingsView()
        : this(true)
    {
    }

    public DocumentScannerTypeSettingsView(bool fromSettings = true)
    {
        BindingContext = new DocumentScannerTypeSettingsViewModel(fromSettings, DoneTcs);
		InitializeComponent();
	}

    private void OnInfoClicked(object sender, EventArgs e)
    {
        ShowInfoPopup(Localization.DocumentScanner, Localization.DocumentScannerDesc);
    }
}
