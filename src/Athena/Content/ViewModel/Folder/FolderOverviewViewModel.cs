using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.TreeView.Engine;

namespace Athena.UI
{
    using DataModel;
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Core;

    public partial class FolderOverviewViewModel : ContextViewModel
    {
        public FolderOverview View { get; set; }

        [ObservableProperty]
        private FolderViewModel _selectedMoveDestination;

        [ObservableProperty]
        private VisualCollection<FolderViewModel, Folder> _moveToFolders;

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
        private bool _isBusy;

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

        internal void LoadData()
        {
            IsBusy = true;

            Task.Run(() =>
            {
                var folders = ParentFolder.Folders.Select(x => new RootItemViewModel(x));
                var documents = ParentFolder.Documents.Select(x => new RootItemViewModel(x));

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    RootSource.AddRange(folders);
                    RootSource.AddRange(documents);
                    IsBusy = false;
                });
            });

        }

        protected override void OnPublishDataStarted()
        {
            IsBusy = true;
        }

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;

                if (e.Folders.Count > 0)
                    ProcessFolderUpdate(e.Folders);
                if (e.Documents.Count > 0)
                    ProcessDocumentUpdate(e.Documents);
                if (e.Tags.Count > 0)
                    ProcessTagsUpdate(e.Tags);

                IsBusy = false;
            });
        }

        private void ProcessFolderUpdate(IList<RequestUpdate<Folder>> folders)
        {
            if (folders[0].Type == UpdateType.Initialize)
            {
                RootSource.AddRange(folders.Select(x => new RootItemViewModel(x)));
            }
            else
            {
                foreach (var folder in folders)
                {
                    if (folder.ParentReference?.Id == ParentFolder.Id)
                    {
                        RootSource.Process(folder, ParentFolder);
                        //break;
                    }
                }
            }
        }

        private void ProcessTagsUpdate(IList<RequestUpdate<Tag>> tags)
        {
            var deletedTagIds = tags
                .Where(x => x.Type == UpdateType.Delete)
                .Select(x => x.Entity.Id)
                .ToHashSet();

            foreach (var document in RootSource.Where(x => !x.IsFolder))
            {
                var validTags = document.Document.Tags.Where(x => !deletedTagIds.Contains(x.Id)).ToList();

                document.Document.Tags.Clear();

                foreach (var tag in validTags)
                {
                    document.Document.Tags.Add(tag);
                }
            }
        }

        private void ProcessDocumentUpdate(IList<RequestUpdate<Document>> documents)
        {
            if (documents[0].Type == UpdateType.Initialize)
            {
                RootSource.AddRange(documents.Select(x => new RootItemViewModel(x)));
            }
            else
            {
                foreach (var document in documents)
                {
                    if (document.Handled)
                        continue;

                    if (document.ParentReference.Id == ParentFolder.Id)
                    {
                        RootSource.Process(document, ParentFolder);
                    }
                    else
                    {
                        if (document.Type == UpdateType.Add || document.Type == UpdateType.Delete)
                        {
                            Stack<Folder> folders = new Stack<Folder>();

                            foreach (var folder in ParentFolder.LoadedFolders)
                            {
                                folders.Push(folder);
                            }

                            while (folders.Count > 0)
                            {
                                Folder currentFolder = folders.Pop();

                                if (currentFolder.Id == document.ParentReference.Id)
                                {
                                    currentFolder.ResetDocumentsLoaded();
                                    document.Handled = true;
                                    break;
                                }

                                foreach (Folder folder in currentFolder.LoadedFolders)
                                {
                                    folders.Push(folder);
                                }
                            }
                        }
                    }
                }
            }
        }

        [RelayCommand]
        public async Task AddDocument()
        {
            IsAddPopupOpen = false;
            await PushModalAsync(new DocumentEditorView(ParentFolder, null));
        }

        [RelayCommand]
        public async Task AddFolder()
        {
            IsAddPopupOpen = false;
            NewFolder = new FolderViewModel(new Folder());
            await PushAsync(new FolderEditorView(null, ParentFolder));
        }

        [RelayCommand]
        public async Task EditItem(RootItemViewModel item)
        {
            ShowMenuPopup = false;

            if (item.IsFolder)
            {
                await PushAsync(new FolderEditorView(item.Folder, ParentFolder));
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

                ServiceProvider.GetService<IDataBrokerService>().Publish<Folder>(
                    RetrieveContext(),
                    item.Folder,
                    UpdateType.Edit,
                    ParentFolder.Key);
            }
            else
            {
                item.Document.Document.Save(RetrieveContext());

                ServiceProvider.GetService<IDataBrokerService>().Publish<Document>(
                    RetrieveContext(),
                    item.Document,
                    UpdateType.Edit,
                    ParentFolder.Key);
            }

            View.RefreshListViewGrouping();
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

            var context = RetrieveContext();

            if (item.IsFolder)
            {
                item.Folder.Folder.Delete(context);



                ServiceProvider.GetService<IDataBrokerService>().Publish<Folder>(
                    context,
                    item.Folder,
                    UpdateType.Delete,
                    ParentFolder.Key);
            }
            else
            {
                item.Document.Document.Delete(context);

                ServiceProvider.GetService<IDataBrokerService>().Publish<Document>(
                    context,
                    item.Document,
                    UpdateType.Delete,
                    ParentFolder.Key);
            }

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

            FolderViewModel rootFolder = ServiceProvider.GetService<IDataBrokerService>().GetRootFolder();
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
                return;

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

            var documentViewModel = SelectedItem.Document;
            var context = RetrieveContext();

            documentViewModel.Document.MoveTo(context, ParentFolder.Folder, SelectedMoveDestination.Folder);

            // TODO: fix adding and removing
            IDataBrokerService publishService = ServiceProvider.GetService<IDataBrokerService>();

            publishService.Publish<Document>(
                context,
                documentViewModel,
                UpdateType.Delete,
                ParentFolder.Key);

            publishService.Publish<Document>(
                context,
                documentViewModel,
                UpdateType.Add,
                SelectedMoveDestination.Folder.Key);

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
