
using Syncfusion.Maui.ListView;

namespace Athena.UI;


public partial class FolderOverview : ContentPage
{
    private bool _firstUsageChecked;
    public FolderOverview()
    {
        BindingContext = new FolderOverviewViewModel();
        InitializeComponent();

        ServiceProvider.GetService<INavigationService>().AsRoot(this);
        _vm = BindingContext as FolderOverviewViewModel;

        this.Loaded += (sender, args) =>
        {
            if (!_firstUsageChecked)
            {
                _vm.CheckFirstUsage();
            }

            _firstUsageChecked = true;
        };
    }
    
    private FolderOverviewViewModel _vm;
    
    
    private void ListView_OnItemLongPress(object sender, ItemLongPressEventArgs e)
    {
        _vm.SelectedFolder = e.DataItem as FolderViewModel;
        menuPopup.Show();
    }

    private void MenuPopup_OnClosed(object sender, EventArgs e)
    {
        _vm.SelectedFolder = null;
    }
}