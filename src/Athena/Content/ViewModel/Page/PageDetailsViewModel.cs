using System.Collections.ObjectModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Maui.Core;
    using CommunityToolkit.Mvvm.Input;

    public partial class PageDetailsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private DocumentViewModel _selectedDocument;

        [ObservableProperty]
        private bool _showMenuPopup;

        [ObservableProperty]
        private bool _showInfoPopup;

        private PageViewModel _page;
        private readonly Folder _folder;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isDocumentMenuOpen;

        [ObservableProperty]
        private bool _isMoveDocumentPopupOpen;

        [ObservableProperty]
        private MoveDocumentTargetViewModel _selectedMoveDestination;

        [ObservableProperty]
        private ObservableCollection<MoveDocumentTargetViewModel> _folders;

        public PageViewModel Page
        {
            get { return _page; }
            set
            {
                _page = value;
                OnPropertyChanged();
            }
        }

        private readonly Page _dummyPage;
        private readonly IEnumerable<FolderViewModel> _allFolders;

        public PageDetailsViewModel(Folder folder, Page page, IEnumerable<FolderViewModel> allFolders)
        {
            _allFolders = allFolders;
            this._dummyPage = page;
            this._folder = folder;
        }

        internal void LoadDocumentOverview()
        {
            IsBusy = true;

            Task.Run(() =>
            {
                // Force load
                _ = _dummyPage.Documents.Count;
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Page = _dummyPage;
                    IsBusy = false;
                });
            });
        }

        [RelayCommand]
        private void InfoClicked()
        {
            ShowMenuPopup = false;
            ShowInfoPopup = true;
        }

        [RelayCommand]
        private async Task DeleteClicked()
        {
            ShowMenuPopup = false;

            bool result = await DisplayAlert(
                Localization.DeletePage,
                string.Format(Localization.DeletePageConfirm, Page.Title),
                Localization.Yes,
                Localization.No);


            if (!result)
                return;

            var context = this.RetrieveContext();

            Page.Page.Delete(context);
            ServiceProvider.GetService<IDataBrokerService>().Publish<Page>(context, Page, UpdateType.Remove, this._folder.Key);

            await Toast.Make(string.Format(Localization.PageDeleted, Page.Title), ToastDuration.Long).Show();
            await PopAsync();

        }

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            var pageUpdate = e.Pages.FirstOrDefault(x => x.Entity == this.Page.Page);

            if (pageUpdate != null)
            {
                if (pageUpdate.Type == UpdateType.Edit)
                {
                    Page.Title = pageUpdate.Entity.Title;
                    Page.Comment = pageUpdate.Entity.Comment;
                }
            }

            var documentUpdate = e.Documents.Where(x => x.ParentReference == this.Page.Page.Key);

            foreach (var docUpdate in documentUpdate)
            {
                if (docUpdate.Type == UpdateType.Add)
                {
                    Page.AddDocument(docUpdate);
                }
                else if (docUpdate.Type == UpdateType.Remove)
                {
                    var document = this.Page.Documents.FirstOrDefault(x => x.Document.Id == docUpdate.Entity.Id);

                    if (document != null)
                        Page.RemoveDocument(document);
                }
            }
        }

        [RelayCommand]
        private async Task AddDocument()
        {
            await PushModalAsync(new DocumentEditorView(this._folder, _page, null));
        }

        [RelayCommand]
        private async Task DeleteDocument(DocumentViewModel document)
        {
            bool result = await DisplayAlert(
                Localization.DeleteDocument,
                string.Format(Localization.DeleteDocumentConfirm, document.Name),
                Localization.Yes,
                Localization.No);

            if (!result)
                return;

            var context = this.RetrieveContext();

            document.Document.Delete(context);
            Page.RemoveDocument(document);

            await Toast.Make(string.Format(Localization.DocumentDeleted, document.Name), ToastDuration.Long).Show();

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, document.Document, UpdateType.Remove, Page.Page.Key);
            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Page.Page, UpdateType.Edit, _folder.Key);
            IsDocumentMenuOpen = false;
            SelectedDocument = null;
        }

        [RelayCommand]
        private async Task DocumentSelected(DocumentViewModel document)
        {
            await PushAsync(new DocumentDetailsView(_page, document));
            SelectedDocument = null;
        }

        [RelayCommand]
        private async Task EditDocument(DocumentViewModel document)
        {
            await PushModalAsync(new DocumentEditorView(this._folder, _page, document));
            SelectedDocument = null;
            IsDocumentMenuOpen = false;
        }

        [RelayCommand]
        private void MoveDocument()
        {
            if (_folders == null || _folders.Count == 0)
            {
                IsBusy = true;
                Folders = new();

                foreach (var folder in _allFolders)
                {
                    MoveDocumentTargetViewModel folderVm = new (folder.Name, folder.Comment, true, null);

                    foreach (var page in folder.Pages)
                    {
                        MoveDocumentTargetViewModel pageVm = new (page.Title, page.Comment, false, page);
                        folderVm.Children.Add(pageVm);
                    }

                    Folders.Add(folderVm);
                }

                IsBusy = false;
            }

            IsMoveDocumentPopupOpen = true;
        }

        [RelayCommand]
        private async Task MoveDestinationSelected()
        {
            if (SelectedMoveDestination == null)
                return;

            if (SelectedMoveDestination.IsFolder)
            {
                SelectedMoveDestination = null;
                return;
            }

            if (SelectedMoveDestination.Page.Id == this.Page.Page.Id)
                return;

            bool move = await DisplayAlert(
                Localization.MoveDocument,
                string.Format(Localization.MoveDocumentConfirm, SelectedDocument.Name, SelectedMoveDestination.Name),
                Localization.Yes,
                Localization.No);

            if (!move)
            {
                SelectedMoveDestination = null;
                return;
            }

            IContext context = this.RetrieveContext();

            SelectedDocument.Document.MoveTo(context, _page.Page, SelectedMoveDestination.Page);

            IDataBrokerService publishService = ServiceProvider.GetService<IDataBrokerService>();
            publishService.Publish(context, SelectedDocument.Document, UpdateType.Remove, Page.Page.Key);
            publishService.Publish(context, SelectedDocument.Document, UpdateType.Add, SelectedMoveDestination.Page.Key);

            await Toast.Make(
                    string.Format(Localization.DocumentMovedSuccessfully, SelectedDocument.Name, SelectedMoveDestination.Name), 
                    ToastDuration.Long)
                .Show();

            Page.RemoveDocument(SelectedDocument);
            SelectedMoveDestination.Page.AddDocument(SelectedDocument);

            SelectedDocument = null;
            SelectedMoveDestination = null;
            IsMoveDocumentPopupOpen = false;
            IsDocumentMenuOpen = false;
        }
    }
}
