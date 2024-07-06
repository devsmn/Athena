
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;
using System;

namespace Athena.UI;



public partial class FolderOverview : ContentPage
{
    private bool _firstUsageChecked;
    private readonly FolderOverviewViewModel _viewModel;

    public FolderOverview()
    {
        BindingContext = new FolderOverviewViewModel(); // ROOT view
        InitializeComponent();

        ServiceProvider.GetService<INavigationService>().AsRoot(this);
        _viewModel = BindingContext as FolderOverviewViewModel;

        Loaded += (_, _) =>
        {
            if (!_firstUsageChecked)
            {
                _viewModel.CheckFirstUsage();
            }

            _firstUsageChecked = true;
        };

        _viewModel.View = this; 
    }

    public FolderOverview(FolderViewModel parentFolder)
    {
        BindingContext = new FolderOverviewViewModel(parentFolder);
        InitializeComponent();

        _viewModel = BindingContext as FolderOverviewViewModel;
        _viewModel.View = this;
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

    public void RefreshListViewGrouping()
    {
        //listView.RefreshView();
    }
}