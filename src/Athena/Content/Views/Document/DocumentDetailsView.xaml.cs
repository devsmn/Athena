using Syncfusion.Maui.Popup;

namespace Athena.UI
{
    using Athena.DataModel;


    public partial class DocumentDetailsView : ContentPage
    {
        public DocumentDetailsView(Page page, Document document)
        {
            var vm = new DocumentDetailsViewModel(page, document);
            this.BindingContext = vm;

            InitializeComponent();
        }

        public DocumentDetailsView(Page page, Chapter chapter)
        {
            this.BindingContext = new DocumentDetailsViewModel(page, chapter);
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            menuPopup.ShowRelativeToView(this.Content, PopupRelativePosition.AlignTopRight);
        }
    }
}