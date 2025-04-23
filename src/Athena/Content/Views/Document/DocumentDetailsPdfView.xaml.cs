namespace Athena.UI;


public partial class DocumentDetailsPdfView : ContentPage
{
    public DocumentDetailsPdfView(byte[] pdf)
    {
        InitializeComponent();
        pdfViewer.DocumentSource = pdf;

        for (int i = 0; i < pdfViewer.Toolbars.Count; i++)
        {
            pdfViewer.Toolbars[i].IsVisible = false;
        }
    }

    public DocumentDetailsPdfView(byte[] pdf, int pageNumber)
        : this(pdf)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pdfViewer.GoToPage(pageNumber);
    }
}