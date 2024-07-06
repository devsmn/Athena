using Syncfusion.Maui.Core;
using System.Collections.ObjectModel;

namespace Athena.UI;

public partial class SearchOverviewView : ContentPage
{
    private readonly SearchChapterViewModel _vm;

	public SearchOverviewView()
    {
        _vm = new SearchChapterViewModel();
        BindingContext = _vm;
		InitializeComponent();
        _vm.View = this;
    }

    private void SfChipGroup_OnSelectionChanged(object sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (sender is not SfChipGroup chips)
            return;

        if (chips.SelectedItem is not ObservableCollection<object> list)
            return;

        _vm.IsFullTextSearch = list.Count == 1;
    }
}