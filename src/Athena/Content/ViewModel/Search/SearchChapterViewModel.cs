using System.Collections.ObjectModel;
using System.ComponentModel;
using Athena.DataModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IUtf8SpanFormattable = System.IUtf8SpanFormattable;

namespace Athena.UI
{
    public partial class SearchChapterViewModel : ContextViewModel
    {
        private int _isSearchActive;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _isFullTextSearch;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _showFilterPopup;

        //private ObservableCollection<TagViewModel> _tags;

        [ObservableProperty]
        private VisualCollection<TagViewModel, Tag> _tags;

        [ObservableProperty]
        private VisualCollection<TagViewModel, Tag> _selectedTags;

        private ObservableCollection<SearchResult> _searchResult;

        public ObservableCollection<SearchResult> SearchResult
        {
            get { return _searchResult; }
            set
            {
                _searchResult = value;
                OnPropertyChanged();
            }
        }

        public ContentPage View { get; set; }

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            if (!e.Tags.Any())
                return;

            Application.Current.Dispatcher.Dispatch(() => {
                foreach (var tagUpdate in e.Tags)
                {
                    Tags.Process(tagUpdate);
                }
            });
        }

        public SearchChapterViewModel()
        {
            //Tags = new ObservableCollection<Tag>(Tag.ReadAll(this.RetrieveContext()));
            SelectedTags = new();
            Tags = new (Tag.ReadAll(this.RetrieveContext()).Select(x => new TagViewModel(x)));
        }
        

        [RelayCommand]
        private async Task StartSearch()
        {
            await Search(SearchText);
        }

        [RelayCommand]
        private void FilterClicked()
        {
            ShowFilterPopup = true;
        }

        private async Task Search(string text)
        {
            if (Interlocked.Exchange(ref _isSearchActive, 1) == 1)
                return;

            IsBusy = true;

            await Task.Run(() =>
            {
                var context = RetrieveContext();

                var results = Document.Search(context, text, SelectedTags.Select(x => x.Tag),  IsFullTextSearch);

                MainThread.InvokeOnMainThreadAsync(() => {
                    SearchResult = new ObservableCollection<SearchResult>(results);
                });
            });

            IsBusy = false;
            await this.View.Navigation.PushAsync(new SearchResultView(this));
            Interlocked.Exchange(ref _isSearchActive, 0);
        }

    }
}
