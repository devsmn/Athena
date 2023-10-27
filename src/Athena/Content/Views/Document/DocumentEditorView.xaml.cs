namespace Athena.UI
{
    using Athena.DataModel;

    public partial class DocumentEditorView : ContentPage
    {

        public DocumentEditorView(Folder folder, Page page, Document document)
        {
            this.BindingContext = new DocumentEditorViewModel(folder, page, document);
            InitializeComponent();
        }
    }
}