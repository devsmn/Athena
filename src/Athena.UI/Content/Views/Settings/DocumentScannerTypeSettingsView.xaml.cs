namespace Athena.UI;

public partial class DocumentScannerTypeSettingsView : ContentPage
{
	public DocumentScannerTypeSettingsView()
    {
        BindingContext = new DocumentScannerTypeSettingsViewModel();
		InitializeComponent();
	}
}
