using Athena.Resources.Localization;
using Syncfusion.Compression.Zip;

namespace Athena.UI;

public partial class DocumentScannerTypeSettingsView : DefaultContentPage
{
    public const string Route = "settings/documentscanner";
    private readonly DocumentScannerTypeSettingsViewModel vm;

    public DocumentScannerTypeSettingsView()
        : this(true)
    {
    }

    public DocumentScannerTypeSettingsView(bool fromSettings = true)
    {
        BindingContext = new DocumentScannerTypeSettingsViewModel(fromSettings, DoneTcs);
        InitializeComponent();
        vm = BindingContext as DocumentScannerTypeSettingsViewModel;
    }

    private void OnInfoClicked(object sender, EventArgs e)
    {
        ShowInfoPopup(Localization.DocumentScanner, Localization.DocumentScannerDesc);
    }

    protected override bool OnBackButtonPressed()
    {
        vm.BackPressed();
        return true;
    }
}
