namespace Athena.UI;


public partial class DocumentDetailsPdfView : ContentPage
{
    public DocumentDetailsPdfView(byte[] pdf)
    {
        InitializeComponent();
        pdfViewer.DocumentSource = pdf;
    }

    public DocumentDetailsPdfView(byte[] pdf, int pageNumber)
        : this(pdf)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pdfViewer.GoToPage(pageNumber);
    }
}