using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.UI;

public partial class HomeView : ContentPage
{
    private bool _dataInitialized;
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
        if (_dataInitialized)
            return;

        await _viewModel.CheckFirstUsage();
        await _viewModel.InitializeAsync();
        _dataInitialized = true;
    }
}
