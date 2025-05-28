namespace Athena.UI
{
    using DataModel;

    public partial class DocumentEditorView : DefaultContentPage
    {
        public DocumentEditorView(Folder folder, Document document)
        {
            BindingContext = new DocumentEditorViewModel(folder, document);
            InitializeComponent();
            Loaded += OnLoaded;
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as DocumentEditorViewModel).BackButton();
            return true;
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            await (BindingContext as DocumentEditorViewModel).PrepareAd();
        }
    }
}
