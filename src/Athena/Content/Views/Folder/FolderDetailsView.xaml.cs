using System.Diagnostics;
using Athena.DataModel;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;

namespace Athena.UI;


public partial class FolderDetailsView : ContentPage
{

    private FolderDetailsViewModel _vm;

    public FolderDetailsView(Folder selectedFolder)
    {
        _vm = new FolderDetailsViewModel(selectedFolder);
        this.BindingContext = _vm;
        InitializeComponent();

        _vm.LoadPages();
    }
    
    private void MenuItem_OnClicked(object sender, EventArgs e)
    {
        menuPopup.ShowRelativeToView(this.Content, PopupRelativePosition.AlignTopRight);
    }

    private void PageList_OnItemLongPress(object sender, ItemLongPressEventArgs e)
    {
        (this.BindingContext as FolderDetailsViewModel).SelectedPage = e.DataItem as PageViewModel;
        pageMenuPopup.Show();
    }

    private void PageMenuPopup_OnClosed(object sender, EventArgs e)
    {
        _vm.SelectedPage = null;
    }
}