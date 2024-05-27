using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Athena.Resources.Localization;

namespace Athena.UI
{
    using Athena.DataModel;
    using Athena.DataModel.Core;
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Core;

    public partial class FolderOverviewViewModel : ContextViewModel
    {
        [ObservableProperty]
        private string searchBarText;

        [ObservableProperty]
        private byte[] newDocumentImage;

        [ObservableProperty]
        private Document newDocument;

        [ObservableProperty]
        private int addDocumentStep;

        [ObservableProperty]
        private Page selectedPage;

        [ObservableProperty]
        private int newFolderStep;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private int addPageStep;

        [ObservableProperty]
        private FolderViewModel newFolder;

        [ObservableProperty]
        private bool _showMenuPopup;

        [ObservableProperty]
        private byte[] newFolderImage;

        [ObservableProperty]
        private Page newPage;

        [ObservableProperty]
        public FolderViewModel selectedFolder;

        [ObservableProperty]
        public ObservableCollection<Document> documents;

        [ObservableProperty]
        public ObservableCollection<FolderViewModel> folders;
        
        [ObservableProperty]
        private string _busyText;

        private bool firstUsage;
        
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
            //BusyText = "Loading folders...";
        }

        public async Task CheckFirstUsage()
        {
            IPreferencesService prefService = ServiceProvider.GetService<IPreferencesService>();

            firstUsage = prefService.IsFirstUsage();

            if (firstUsage)
            {
                prefService.SetFirstUsage(); 
                await PushModalAsync(new TutorialView());
            }
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
        

        //[RelayCommand]
        //private async Task AddPage(FolderViewModel folder)
        //{
        //    NewPage = new Page();
        //    AddPageStep = 0;

        //    await PushAsync(new PageEditorView(new Page(), folder));
        //}

        //[RelayCommand]
        //private async Task CloseFolderAddButtonActionPage()
        //{
        //    await PopModalAsync();
        //}

        [RelayCommand]
        private async Task ShowPageAddActions()
        {
            NewFolder = new FolderViewModel(new Folder());
            await PushAsync(new FolderEditorView(null));
        }

        //[RelayCommand]
        //private async Task AddPageDocument()
        //{
        //    Document document = new Document();

        //    if (MediaPicker.Default.IsCaptureSupported)
        //    {
        //        FileResult image = await MediaPicker.Default.CapturePhotoAsync();

        //        if (image != null)
        //        {
        //            using Stream sourceStream = await image.OpenReadAsync();

        //            using MemoryStream byteStream = new();
        //            await sourceStream.CopyToAsync(byteStream);
        //            byte[] bytes = byteStream.ToArray();
                    
        //            document.Pdf = bytes;

        //            NewPage.AddDocument(document);
        //        }
        //    }
        //}

        [RelayCommand]
        public async Task FolderClicked()
        {
            await PushAsync(new FolderDetailsView(SelectedFolder));
            SelectedFolder = null;
        }

        //[RelayCommand]
        //private async Task AddNewFolder()
        //{
        //    NewFolder = new FolderViewModel(new Folder());
        //    await PopModalAsync();
        //    await PushAsync(new FolderEditorView(null));
        //}

        //[RelayCommand]
        //private async Task SaveFolder()
        //{
        //    IContext context = this.RetrieveContext();
            
        //    NewFolder.Folder.Save(context);

        //    await PopAsync();
        //    ServiceProvider.GetService<IDataBrokerService>().Publish(context, NewFolder.Folder, UpdateType.Add);
        //}

        //[RelayCommand]
        //private async Task CaptureImage()
        //{
        //    if (MediaPicker.Default.IsCaptureSupported)
        //    {
        //        IContext context = this.RetrieveContext();

        //        context.Log("Taking picture");

        //        FileResult image = await MediaPicker.Default.CapturePhotoAsync();

        //        if (image != null)
        //        {
        //            using Stream sourceStream = await image.OpenReadAsync();

        //            using MemoryStream byteStream = new();
        //            await sourceStream.CopyToAsync(byteStream);
        //            byte[] bytes = byteStream.ToArray();

        //            string content = Convert.ToBase64String(bytes);

        //            Document doc = new Document(new DocumentKey(DocumentKey.TemporaryId));

        //            doc.Pdf = bytes;
        //            doc.Name = "test";
        //            doc.Comment = " comment";

        //            doc.Save(context);

        //            ServiceProvider.GetService<IDataBrokerService>().Publish(context, doc, UpdateType.Add);
        //        }
        //    }
        //}
        
    }
}
