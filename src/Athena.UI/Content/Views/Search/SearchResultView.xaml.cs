using Athena.DataModel;
using Athena.DataModel.Core;
using Syncfusion.Maui.ListView;

namespace Athena.UI;

public partial class SearchResultView : ContentPage
{
    private readonly SearchChapterViewModel _vm;

    public SearchResultView(SearchChapterViewModel vm)
    {
        _vm = vm;
        BindingContext = vm;
        InitializeComponent();
    }

    private void SearchResultView_OnSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
    {
        if (searchResultView.SelectedItem is not SearchResult result)
            return;

        Document doc = Document.Read(new AthenaAppContext(), result.Document.Key);
        Navigation.PushAsync(new DocumentDetailsView(null, doc));
        searchResultView.SelectedItem = null;
    }
}
