namespace Athena.UI
{
    using Athena.DataModel;

    public partial class DocumentDetailsView : ContentPage
    {
        public DocumentDetailsView(Page page, Document document)
        {
            this.BindingContext = new DocumentDetailsViewModel(page, document);
            InitializeComponent();
        }
    }
}