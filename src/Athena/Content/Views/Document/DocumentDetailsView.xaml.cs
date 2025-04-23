using Syncfusion.Maui.Popup;

namespace Athena.UI
{
    using DataModel;


    public partial class DocumentDetailsView : ContentPage
    {
        public DocumentDetailsView(Folder parentFolder, Document document)
        {
            var vm = new DocumentDetailsViewModel(parentFolder, document);
            BindingContext = vm;

            InitializeComponent();
        }

        public DocumentDetailsView(Chapter chapter)
        {
            BindingContext = new DocumentDetailsViewModel(chapter);
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            menuPopup.ShowRelativeToView(Content, PopupRelativePosition.AlignTopRight);
        }
    }
}