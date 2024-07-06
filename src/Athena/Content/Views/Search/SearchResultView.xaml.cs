using Athena.DataModel;
using Syncfusion.Maui.ListView;

namespace Athena.UI;

public partial class SearchResultView : ContentPage
{
    private readonly SearchChapterViewModel _vm;

    public SearchResultView(SearchChapterViewModel vm)
    {
        this._vm = vm;
        BindingContext = vm;
        InitializeComponent();
    }

    private void SearchResultView_OnSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
    {
        if (searchResultView.SelectedItem is not SearchResult result)
            return;

        Document doc = Document.Read(new AthenaAppContext(), result.Document.Key);
        this.Navigation.PushAsync(new DocumentDetailsView(doc));
        this.searchResultView.SelectedItem = null;
    }
}