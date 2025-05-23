using Syncfusion.Maui.Popup;

namespace Athena.UI
{
    using DataModel;


    public partial class DocumentDetailsView : DefaultContentPage
    {
        private DocumentDetailsViewModel _vm;

        public DocumentDetailsView(Folder parentFolder, Document document)
        {
            _vm = new DocumentDetailsViewModel(parentFolder, document);
            BindingContext = _vm;

            InitializeComponent();
        }

        public DocumentDetailsView(Chapter chapter)
        {
            _vm = new DocumentDetailsViewModel(chapter);
            BindingContext = _vm;
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            menuPopup.ShowRelativeToView(Content, PopupRelativePosition.AlignTopRight);
        }
    }
}
