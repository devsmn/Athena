namespace Athena.UI;

public partial class DocumentScannerTypeSettingsView : ContextContentPage
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
}
