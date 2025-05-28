using System.Collections.ObjectModel;
using Athena.DataModel;
using Athena.DataModel.Core;
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

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            if (!e.Tags.Any() || _tags == null)
                return;

            Application.Current.Dispatcher.Dispatch(() =>
            {
                foreach (var tagUpdate in e.Tags)
                {
                    Tags.Process(tagUpdate);
                }
            });
        }

        public SearchChapterViewModel()
        {
            SelectedTags = new();
        }

        public override async Task InitializeAsync()
        {
            await ExecuteBackgroundAction(context =>
            {
                var tags = Tag.ReadAll(context).Select(x => new TagViewModel(x));
                Tags = new(tags);
            });

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

            await ExecuteBackgroundAction(context =>
            {
                var results = Document.Search(context, text, SelectedTags.Select(x => x.Tag), IsFullTextSearch);
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    SearchResult = new ObservableCollection<SearchResult>(results);
                });
            });

            await View.Navigation.PushAsync(new SearchResultView(this));
            Interlocked.Exchange(ref _isSearchActive, 0);
        }

    }
}
