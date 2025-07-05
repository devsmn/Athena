using Athena.Resources.Localization;

namespace Athena.UI;

public partial class DocumentEditorDocumentStep : ContentView
{
    public DocumentEditorDocumentStep()
    {
        InitializeComponent();
    }

    private void AiTextInfoClicked(object sender, EventArgs e)
    {
        DefaultContentPage page = this.GetParent<DefaultContentPage>();
        page?.ShowInfoPopup(Localization.DocumentEditorAIHeader, Localization.DocumentEditorAIDescription);
    }
}
