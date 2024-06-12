using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using Syncfusion.Maui.TreeView;
using Syncfusion.TreeView.Engine;
using ItemLongPressEventArgs = Syncfusion.Maui.ListView.ItemLongPressEventArgs;

namespace Athena.UI
{
    using Athena.DataModel;
    using Syncfusion.Maui.Popup;


    public partial class PageDetailsView : ContentPage
    {
        private PageDetailsViewModel _vm;

        public PageDetailsView(Folder folder, Page page, IEnumerable<FolderViewModel> allFolders)
        {
            _vm = new PageDetailsViewModel(folder, page, allFolders);
            this.BindingContext = _vm;
            InitializeComponent();

            this.NavigatedTo += (s, e) =>
            {
                //sfPopup.IsOpen = true;
                _vm.LoadDocumentOverview();
            };
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            menuPopup.ShowRelativeToView(this.Content, PopupRelativePosition.AlignTopRight);
        }

        private void SfPopup_OnClosed(object sender, EventArgs e)
        {
            _vm.SelectedDocument = null;
        }

        private void CollectionView_OnItemLongPress(object sender, ItemLongPressEventArgs e)
        {
            _vm.SelectedDocument = e.DataItem as DocumentViewModel;
            documentMenuPopup.Show();
        }

        private void DocumentMenuPopup_OnClosed(object sender, EventArgs e)
        {
            _vm.SelectedDocument = null;
        }

        private void MoveDocumentPopupClosed(object sender, EventArgs e)
        {
            _vm.IsDocumentMenuOpen = false;
            //_vm.SelectedDocument = null;
        }


    }
}
