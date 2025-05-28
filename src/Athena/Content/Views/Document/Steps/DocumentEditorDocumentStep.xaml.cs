using System.Diagnostics;
using Athena.Resources.Localization;

namespace Athena.UI;


public partial class DocumentEditorDocumentStep : ContentView
{
    public DocumentEditorDocumentStep()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine($"DocumentEditorDocumentStep: {BindingContext?.GetType()?.FullName}");

        BindingContextChanged += (sender, args) =>
        {
            Debug.WriteLine("Binding context changed: " + BindingContext.GetType().FullName);
        };
    }

    private void AiTextInfoClicked(object sender, EventArgs e)
    {
        DefaultContentPage page = this.GetParent<DefaultContentPage>();

        if (page == null)
            return;

        page.ShowInfoPopup(Localization.DocumentEditorAIHeader, Localization.DocumentEditorAIDescription);
    }
}
