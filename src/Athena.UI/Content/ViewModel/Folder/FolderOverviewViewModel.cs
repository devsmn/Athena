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
                IEnumerable<RootItemViewModel> folders = ParentFolder.Folders.Select(x => new RootItemViewModel(x));
                IEnumerable<RootItemViewModel> documents = ParentFolder.Documents.Select(x => new RootItemViewModel(x));

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

        protected override void OnDataPublished(DataPublishedArgs data)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;

                if (data.Folders.Count > 0)
                    ProcessFolderUpdate(data.Folders);
                if (data.Documents.Count > 0)
                    ProcessDocumentUpdate(data.Documents);
                if (data.Tags.Count > 0)
                    ProcessTagsUpdate(data.Tags);

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
                foreach (RequestUpdate<Folder> folder in folders)
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
            HashSet<int> deletedTagIds = tags
                .Where(x => x.Type == UpdateType.Delete)
                .Select(x => x.Entity.Id)
                .ToHashSet();

            foreach (RootItemViewModel document in RootSource.Where(x => !x.IsFolder))
            {
                List<Tag> validTags = document.Document.Tags.Where(x => !deletedTagIds.Contains(x.Id)).ToList();

                document.Document.Tags.Clear();

                foreach (Tag tag in validTags)
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
                foreach (RequestUpdate<Document> document in documents)
                {
                    if (document.Handled)
                        continue;

                    // If the document is part of our folder, we can just update it.
                    if (document.ParentReference.Id == ParentFolder.Id)
                    {
                        RootSource.Process(document, ParentFolder);
                    }
                    else
                    {
                        // The document might be inside one of our subfolders.
                        // Try to find it and ensure the documents are reloaded the next time the folder is opened.
                        // This only applies to when documents are moved.
                        // For other update types, the RootSource.Process will eventually be called on the right parent folder.
                        if (document.Type == UpdateType.Move)
                        {
                            Stack<Folder> folders = new Stack<Folder>();

                            foreach (Folder folder in ParentFolder.LoadedFolders)
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

                Services.GetService<IDataBrokerService>().Publish<Folder>(
                    RetrieveContext(),
                    item.Folder,
                    UpdateType.Edit,
                    ParentFolder.Key);
            }
            else
            {
                item.Document.Document.Save(RetrieveContext());

                Services.GetService<IDataBrokerService>().Publish<Document>(
                    RetrieveContext(),
                    item.Document,
                    UpdateType.Edit,
                    ParentFolder.Key);
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

            if (item.IsFolder)
            {
                item.Folder.Folder.Delete(context);

                Services.GetService<IDataBrokerService>().Publish<Folder>(
                    context,
                    item.Folder,
                    UpdateType.Delete,
                    ParentFolder.Key);
            }
            else
            {
                item.Document.Document.Delete(context);

                Services.GetService<IDataBrokerService>().Publish<Document>(
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

            FolderViewModel rootFolder = Services.GetService<IDataBrokerService>().GetRootFolder();
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

            DocumentViewModel documentViewModel = SelectedItem.Document;
            IContext context = RetrieveContext();

            documentViewModel.Document.MoveTo(context, ParentFolder.Folder, SelectedMoveDestination.Folder);

            IDataBrokerService publishService = Services.GetService<IDataBrokerService>();

            publishService.Publish<Document>(
                context,
                documentViewModel,
                UpdateType.Delete, // Delete, handled by current folder
                ParentFolder.Key);

            publishService.Publish<Document>(
                context,
                documentViewModel,
                UpdateType.Move, // Do not use Add because the current folder is not the same as the one where the document is moved to
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
