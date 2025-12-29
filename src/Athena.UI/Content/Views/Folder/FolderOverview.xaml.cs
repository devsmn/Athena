using Syncfusion.Maui.Popup;
using ItemLongPressEventArgs = Syncfusion.Maui.ListView.ItemLongPressEventArgs;

namespace Athena.UI;

public partial class FolderOverview : ContentPage
{
    public const string Route = "rootitem";
    public const string FolderParameter = "Folder";

    private readonly FolderOverviewViewModel _viewModel;

    public FolderOverview()
    {
        BindingContext = new FolderOverviewViewModel();
        InitializeComponent();

        _viewModel = BindingContext as FolderOverviewViewModel;
    }

    private void ListView_OnItemLongPress(object sender, ItemLongPressEventArgs e)
    {
        _viewModel.SelectedItem = e.DataItem as RootItemViewModel;
        menuPopup.Show();
    }

    private void MenuPopup_OnClosed(object sender, EventArgs e)
    {
        _viewModel.SelectedItem = null;
    }

    private void Button_OnClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        addPopup.ShowRelativeToView(button, PopupRelativePosition.AlignTop);
    }

    private void MoveDocumentPopupClosed(object sender, EventArgs e)
    {
        _viewModel.ShowMenuPopup = false;
    }
}
