using System.ComponentModel;
using System.Diagnostics;

namespace Athena.UI
{
    using Athena.DataModel;


    public partial class DocumentEditorView : ContentPage
    {
        public  DocumentEditorView(Folder folder, Page page, Document document)
        {
            this.BindingContext = new DocumentEditorViewModel(folder, page, document);
            InitializeComponent();

            this.Loaded += OnLoaded;

        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            await (this.BindingContext as DocumentEditorViewModel).PrepareAd();
        }
    }
}