using System.Diagnostics;

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
}
