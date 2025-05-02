using Athena.DataModel.Core;

namespace Athena.UI;

public partial class HomeView : ContentPage
{
    private bool _firstUsageChecked;
    private readonly HomeViewModel _viewModel;

    public HomeView()
    {
        InitializeComponent();
        Services.GetService<INavigationService>().AsRoot(this);

        _viewModel = BindingContext as HomeViewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        if (_firstUsageChecked)
            return;

        _firstUsageChecked = true;
        await _viewModel.CheckFirstUsage();
    }
}
