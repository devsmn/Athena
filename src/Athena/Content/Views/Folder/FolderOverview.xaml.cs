
using Syncfusion.Maui.ListView;

namespace Athena.UI;


public partial class FolderOverview : ContentPage
{
    private bool firstUsageChecked;
    public FolderOverview()
    {
        BindingContext = new FolderOverviewViewModel();
        InitializeComponent();

        ServiceProvider.GetService<INavigationService>().AsRoot(this);
        vm = BindingContext as FolderOverviewViewModel;

        //ServiceProvider.GetService<IDataBrokerService>().RaiseAppInitialized();

        this.Loaded += (sender, args) => {
            if (!firstUsageChecked)
            {
                vm.CheckFirstUsage();
            }

            firstUsageChecked = true;

        };
    }
    
    private FolderOverviewViewModel vm;
    
    
    private void ListView_OnItemLongPress(object sender, ItemLongPressEventArgs e)
    {
        vm.SelectedFolder = e.DataItem as FolderViewModel;
        menuPopup.Show();
    }

    private void MenuPopup_OnClosed(object sender, EventArgs e)
    {
        vm.SelectedFolder = null;
    }
}