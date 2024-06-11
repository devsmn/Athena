using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Athena.Resources.Localization;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Core;

    public partial class FolderOverviewViewModel : ContextViewModel
    {
        [ObservableProperty]
        private string _searchBarText;

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
        private FolderViewModel _selectedFolder;

        [ObservableProperty]
        private ObservableCollection<Document> _documents;

        [ObservableProperty]
        private ObservableCollection<FolderViewModel> _folders;
        
        [ObservableProperty]
        private string _busyText;

        private bool _firstUsage;
        
        public FolderOverviewViewModel()
        {
            var dataService = ServiceProvider.GetService<IDataBrokerService>();

            if (dataService != null)
            {
                dataService.PublishStarted += OnDataBrokerPublishStarted;
                dataService.Published += OnDataBrokerPublished;
            }

            Folders = new ObservableCollection<FolderViewModel>();

        }
       
        private void OnDataBrokerPublishStarted(object sender, EventArgs e)
        {
            IsBusy = true;
        }

        public async Task CheckFirstUsage()
        {
            IPreferencesService prefService = ServiceProvider.GetService<IPreferencesService>();

            _firstUsage = prefService.IsFirstUsage();

            if (_firstUsage)
            {
                prefService.SetFirstUsage();
                await PushModalAsync(new TutorialView());
            }
        }

        protected override async Task OnAppInitialized()
        {
            await CheckFirstUsage();
        }

        private void OnDataBrokerPublished(object sender, DataPublishedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (e.Folders.Count > 0)
                {
                    if (e.Folders[0].Type == UpdateType.Initialize)
                    {
                        Folders = new ObservableCollection<FolderViewModel>(
                            e.Folders.Select(x => new FolderViewModel(x)));
                    }
                    else
                    {
                        foreach (var folder in e.Folders)
                        {
                            switch (folder.Type)
                            {
                                case UpdateType.Add:
                                    Folders.Add(new FolderViewModel(folder));
                                    break;
                                case UpdateType.Remove:
                                {
                                    var toDelete = Folders.FirstOrDefault(x => x.Id == folder.Entity.Id);

                                    if (toDelete != null)
                                    {
                                        Folders.Remove(toDelete);
                                    }

                                    break;
                                }

                                case UpdateType.Edit:
                                {
                                    var relatedFolder = Folders.FirstOrDefault(x => x.Id == folder.Entity.Id);

                                    if (relatedFolder != null)
                                    {
                                        relatedFolder.Comment = folder.Entity.Comment;
                                        relatedFolder.Name = folder.Entity.Name;
                                        relatedFolder.IsPinned = folder.Entity.IsPinned;
                                    }

                                    break;
                                }
                                default:
                                    throw new InvalidOperationException();
                            }
                        }
                    }
                }

                IsBusy = false;
            });
        }

        [RelayCommand]
        public async Task EditFolder(FolderViewModel folder)
        {
            ShowMenuPopup = false;
            await PushAsync(new FolderEditorView(folder));
        }

        [RelayCommand]
        public void PinFolder(FolderViewModel folder)
        {
            ShowMenuPopup = false;
            folder.IsPinned = !folder.IsPinned;
            folder.Folder.Save(this.RetrieveContext());

            ServiceProvider.GetService<IDataBrokerService>().Publish(
                this.RetrieveContext(),
                folder.Folder,
                UpdateType.Edit);
        }

        [RelayCommand]
        public async Task DeleteFolder(FolderViewModel folder)
        {
            ShowMenuPopup = false;

            bool result = await DisplayAlert(
                Localization.DeleteFolder,
                string.Format(Localization.DeleteFolderConfirm, folder.Name),
                Localization.Yes,
                Localization.No);
            

            if (!result)
                return;

            var context = this.RetrieveContext();

            ServiceProvider.GetService<IDataBrokerService>().Publish(
                context,
                folder.Folder,
                UpdateType.Remove);

            folder.Folder.Delete(context);
            await Toast.Make(string.Format(Localization.FolderDeleted, folder.Name), ToastDuration.Long).Show();
        }
        
        [RelayCommand]
        private async Task ShowPageAddActions()
        {
            NewFolder = new FolderViewModel(new Folder());
            await PushAsync(new FolderEditorView(null));
        }

        [RelayCommand]
        public async Task FolderClicked()
        {
            await PushAsync(new FolderDetailsView(SelectedFolder));
            SelectedFolder = null;
        }
    }
}
