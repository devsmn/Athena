using System.Collections.ObjectModel;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.TreeView.Engine;

namespace Athena.UI
{
    public partial class FolderOverviewViewModel : ContextViewModel
    {
        [ObservableProperty]
        private FolderViewModel _selectedMoveDestination;

        [ObservableProperty]
        private ObservableCollection<FolderViewModel> _moveToFolders;

        [ObservableProperty]
        private bool _isMoveDocumentPopupOpen;

        [ObservableProperty]
        private FolderViewModel _parentFolder;

        [ObservableProperty]
        private string _searchBarText;

        [ObservableProperty]
        private bool _isAddPopupOpen;

        [ObservableProperty]
        private byte[] _newDocumentImage;

        [ObservableProperty]
        private Document _newDocument;

        [ObservableProperty]
        private int _addDocumentStep;

        [ObservableProperty]
        private Page _selectedPage;

        [ObservableProperty]
        private int _newFolderStep;

        [ObservableProperty]
        private int _addPageStep;

        [ObservableProperty]
        private FolderViewModel _newFolder;

        [ObservableProperty]
        private bool _showMenuPopup;

        [ObservableProperty]
        private byte[] _newFolderImage;

        [ObservableProperty]
        private Page _newPage;

        [ObservableProperty]
        private RootItemViewModel _selectedItem;

        [ObservableProperty]
        private RootItemCollection _rootSource;

        [ObservableProperty]
        private string _busyText;

        public FolderOverviewViewModel(FolderViewModel parentFolder)
        {
            MoveToFolders = new();
            _parentFolder = parentFolder;
            RootSource = new();
        }

        public override async Task InitializeAsync()
        {
            IEnumerable<RootItemViewModel> folders = null;
            IEnumerable<RootItemViewModel> documents = null;
            RootSource.Clear();

            await ExecuteBackgroundAction(context =>
            {
                folders = ParentFolder.Folder.ReadAllFolders(context).Select(x => new RootItemViewModel(x));
                documents = ParentFolder.Folder.ReadAllDocuments(context).Select(x => new RootItemViewModel(x));
            });

            RootSource.AddRange(folders);
            RootSource.AddRange(documents);
        }
       
        [RelayCommand]
        public async Task AddDocument()
        {
            IsAddPopupOpen = false;

            DocumentEditorView view = new DocumentEditorView(ParentFolder, null);
            await PushModalAsync(view);
            object result = await view.DoneWithResult.Task;

            if (result is DocumentViewModel document)
            {
                RootSource.Add(new RootItemViewModel(document));
            }
        }

        [RelayCommand]
        public async Task AddFolder()
        {
            IContext context = RetrieveContext();
            IsAddPopupOpen = false;
            NewFolder = new FolderViewModel(new Folder());
            RootItemViewModel rootItem = new(NewFolder);

            FolderEditorView view = new FolderEditorView(rootItem);
            await PushAsync(view);
            await view.DoneTcs.Task;

            ParentFolder.Folder.AddFolder(context, NewFolder.Folder);
            RootSource.Add(new RootItemViewModel(NewFolder));
        }

        [RelayCommand]
        public async Task EditItem(RootItemViewModel item)
        {
            ShowMenuPopup = false;

            if (item.IsFolder)
            {
                await PushAsync(new FolderEditorView(item));
            }
            else
            {
                await PushModalAsync(new DocumentEditorView(ParentFolder, item.Document));
            }
        }

        [RelayCommand]
        public void PinFolder(RootItemViewModel item)
        {
            ShowMenuPopup = false;
            item.IsPinned = !item.IsPinned;

            if (item.IsFolder)
            {
                item.Folder.Folder.Save(RetrieveContext(), FolderSaveOptions.Folder);
            }
            else
            {
                item.Document.Document.Save(RetrieveContext());
            }
        }

        [RelayCommand]
        public async Task DeleteItem(RootItemViewModel item)
        {
            ShowMenuPopup = false;

            string caption = Localization.DeleteDocument;
            string message = string.Format(Localization.DeleteDocumentConfirm, item.Name);
            string deletedMessage = string.Format(Localization.DocumentDeleted, item.Name);

            if (item.IsFolder)
            {
                caption = Localization.DeleteFolder;
                message = string.Format(Localization.DeleteFolderConfirm, item.Name);
                deletedMessage = string.Format(Localization.FolderDeleted, item.Name);
            }

            bool result = await DisplayAlert(
                caption,
                message,
                Localization.Yes,
                Localization.No);

            if (!result)
                return;

            IContext context = RetrieveContext();
            item.Delete(context);
            RootSource.Remove(item);

            await Toast.Make(deletedMessage, ToastDuration.Long).Show();
        }

        [RelayCommand]
        public async Task ItemClicked()
        {
            if (SelectedItem.IsFolder)
            {
                await PushAsync(new FolderOverview(SelectedItem.Folder));
            }
            else
            {
                await PushAsync(new DocumentDetailsView(ParentFolder, SelectedItem.Document));
            }

            SelectedItem = null;
        }

        [RelayCommand]
        private void MoveDocument()
        {
            MoveToFolders.Clear();

            FolderViewModel rootFolder = Services.GetService<IRootFolderService>().GetRootFolder();
            MoveToFolders.Add(rootFolder);

            IsMoveDocumentPopupOpen = true;
        }

        [RelayCommand]
        private void LoadMoveToFolders(object obj)
        {
            if (obj is not TreeViewNode node)
                return;

            if (node.ChildNodes?.Count > 0)
            {
                node.IsExpanded = true;
                return;
            }

            if (node.Content is not FolderViewModel folder)
                return;

            node.ShowExpanderAnimation = true;

            Application.Current.Dispatcher.Dispatch(() =>
            {
                node.PopulateChildNodes(folder.Folders.Select(x => new FolderViewModel(x)));

                if (node.HasChildNodes)
                    node.IsExpanded = true;

                node.ShowExpanderAnimation = false;
            });
        }

        [RelayCommand]
        private async Task MoveDestinationSelected()
        {
            if (SelectedMoveDestination == null)
                return;

            if (SelectedMoveDestination.Folder.Id == ParentFolder.Id)
            {
                await DisplayAlert(
                    Localization.MoveDocument,
                    $"The document {SelectedItem.Document.Name} already exists in folder {SelectedMoveDestination.Name}",
                    "Ok");

                SelectedMoveDestination = null;
                return;
            }

            bool move = await DisplayAlert(
                Localization.MoveDocument,
                string.Format(Localization.MoveDocumentConfirm, SelectedItem.Document.Name, SelectedMoveDestination.Name),
                Localization.Yes,
                Localization.No);

            if (!move)
            {
                SelectedMoveDestination = null;
                return;
            }

            DocumentViewModel documentViewModel = SelectedItem.Document;
            IContext context = RetrieveContext();

            documentViewModel.Document.MoveTo(context, ParentFolder.Folder, SelectedMoveDestination.Folder);
            RootSource.Remove(SelectedItem);

            await Toast.Make(
                    string.Format(Localization.DocumentMovedSuccessfully, documentViewModel.Name, SelectedMoveDestination.Name),
                    ToastDuration.Long)
                .Show();

            SelectedMoveDestination = null;
            SelectedItem = null;
            IsMoveDocumentPopupOpen = false;
            ShowMenuPopup = false;
        }
    }
}
