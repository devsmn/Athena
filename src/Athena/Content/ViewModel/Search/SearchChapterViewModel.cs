using System.Collections.ObjectModel;
using Athena.DataModel;
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
        private bool _isBusy;

        [ObservableProperty]
        private bool _showFilterPopup;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _tags;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _selectedTags;

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

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            if (e.Tags.Any())
            {
                foreach (var tagUpdate in e.Tags)
                {
                    if (tagUpdate.Type == UpdateType.Add)
                    {
                        Tags.Add(tagUpdate.Entity);
                    }
                    else if (tagUpdate.Type == UpdateType.Remove)
                    {
                        var toRemove = Tags.FirstOrDefault(x => x.Id == tagUpdate.Entity.Id);

                        if (toRemove != null)
                            Tags.Remove(toRemove);
                    }
                    else if (tagUpdate.Type == UpdateType.Edit)
                    {
                        var toEdit = Tags.FirstOrDefault(x => x.Id == tagUpdate.Entity.Id);

                        if (toEdit != null)
                        {
                            toEdit.Name = tagUpdate.Entity.Name;
                        }
                    }
                }

            }
        }

        public SearchChapterViewModel()
        {
            //Tags = new ObservableCollection<Tag>(Tag.ReadAll(this.RetrieveContext()));
            SelectedTags = new ObservableCollection<TagViewModel>();
            Tags = new ObservableCollection<TagViewModel>(Tag.ReadAll(this.RetrieveContext()).Select(x => new TagViewModel(x)));
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
                var context = this.RetrieveContext();

                var results = Document.Search(context, text, SelectedTags.Select(x => x.Tag),  IsFullTextSearch);

                MainThread.InvokeOnMainThreadAsync(() => SearchResult = new ObservableCollection<SearchResult>(results));
            });
            

            IsBusy = false;
            Interlocked.Exchange(ref _isSearchActive, 0);
        }

    }
}
