using System.ComponentModel;
using System.Diagnostics;

namespace Athena.UI
{
    using Athena.DataModel;


    public partial class DocumentEditorView : ContentPage
    {
        public  DocumentEditorView(Folder folder, Document document)
        {
            BindingContext = new DocumentEditorViewModel(folder, document);
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            await (this.BindingContext as DocumentEditorViewModel).PrepareAd();
        }
    }
}