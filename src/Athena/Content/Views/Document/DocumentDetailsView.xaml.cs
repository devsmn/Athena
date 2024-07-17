using Syncfusion.Maui.Popup;

namespace Athena.UI
{
    using Athena.DataModel;


    public partial class DocumentDetailsView : ContentPage
    {
        public DocumentDetailsView(Folder parentFolder, Document document)
        {
            var vm = new DocumentDetailsViewModel(parentFolder, document);
            this.BindingContext = vm;

            InitializeComponent();
        }

        public DocumentDetailsView(Chapter chapter)
        {
            this.BindingContext = new DocumentDetailsViewModel(chapter);
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            menuPopup.ShowRelativeToView(this.Content, PopupRelativePosition.AlignTopRight);
        }
    }
}