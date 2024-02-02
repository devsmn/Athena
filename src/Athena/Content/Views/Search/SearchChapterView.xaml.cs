using Athena.DataModel;
using Syncfusion.Maui.ListView;

namespace Athena.UI
{

    public partial class SearchChapterView : ContentPage
    {
        public SearchChapterView()
        {
            BindingContext = new SearchChapterViewModel();
            InitializeComponent();
        }

        private void SearchResultView_OnSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            if (searchResultView.SelectedItem is not SearchResult result)
                return;

            Document doc = Document.Read(new AthenaAppContext(), result.Document.Key);
            this.Navigation.PushAsync(new DocumentDetailsView(result.Page, doc));
            this.searchResultView.SelectedItem = null;
        }
    }
}