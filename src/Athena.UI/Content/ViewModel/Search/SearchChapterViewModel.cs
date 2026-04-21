using System.Collections.ObjectModel;
using Athena.DataModel;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        private bool _showFilterPopup;

        [ObservableProperty]
        private VisualCollection<TagViewModel, Tag> _tags;

        [ObservableProperty]
        private VisualCollection<TagViewModel, Tag> _selectedTags;

        private ObservableCollection<SearchResult> _searchResult;

        public ObservableCollection<SearchResult> SearchResult
        {
            get => _searchResult;
            set
            {
                _searchResult = value;
                OnPropertyChanged();
            }
        }

        public ContentPage View { get; set; }

        public SearchChapterViewModel()
        {
            SelectedTags = new();
            Tags = new();
        }

        public override async Task InitializeAsync()
        {
            List<TagViewModel> tags = null;

            await ExecuteBackgroundAction(context =>
            {
                tags = Tag.ReadAll(context).Select(x => new TagViewModel(x)).ToList();
            });

            if (tags.Count == Tags.Count)
            {
                // Assume nothing has changed. Tags usually don't change anyway.
                return;
            }

            Tags.Clear();
            SelectedTags.Clear();
            Tags = new(tags);
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

            BusyText = Localization.SearchRunning;
            IEnumerable<SearchResult> results = Array.Empty<SearchResult>();

            await ExecuteBackgroundAction(context =>
            {
                results = Document.Search(context, text, SelectedTags.Select(x => x.Tag), IsFullTextSearch);
            });

            SearchResult = new(results);
            await View.Navigation.PushAsync(new SearchResultView(this));
            Interlocked.Exchange(ref _isSearchActive, 0);
        }
    }
}
