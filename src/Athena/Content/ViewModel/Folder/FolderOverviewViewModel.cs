using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Athena.UI
{
    using Athena.UI;
    using Athena.DataModel;
    using Athena.DataModel.Core;

    public partial class FolderOverviewViewModel : ContextViewModel
    {
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
        private byte[] newFolderImage;

        [ObservableProperty]
        private Page newPage;

        [ObservableProperty]
        public FolderViewModel selectedFolder;

        [ObservableProperty]
        public ObservableCollection<Document> documents;

        [ObservableProperty]
        public ObservableCollection<FolderViewModel> folders;


        public FolderOverviewViewModel()
        {
            var dataService = ServiceProvider.GetService<IDataBrokerService>();

            if (dataService != null)
            {
                dataService.PublishStarted += OnDataBrokerPublishStarted;
                dataService.Published += OnDataBrokerPublished;
            }
        }

        private void OnDataBrokerPublishStarted(object sender, EventArgs e)
        {
            IsBusy = true;
        }

        private void OnDataBrokerPublished(object sender, DataPublishedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() => {
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
                            if (folder.Type == UpdateType.Add)
                            {
                                Folders.Add(new FolderViewModel(folder));
                            }
                            else if (folder.Type == UpdateType.Remove)
                            {
                                Folders.Remove(new FolderViewModel(folder));
                            }
                            else if (folder.Type == UpdateType.Edit)
                            {
                                var relatedFolder = Folders.FirstOrDefault(x => x.Id == folder.Entity.Id);

                                if (relatedFolder != null)
                                {
                                    relatedFolder.Comment = folder.Entity.Comment;
                                    relatedFolder.Name = folder.Entity.Name;
                                    relatedFolder.PageCount = folder.Entity.PageCount;
                                    relatedFolder.Thumbnail = folder.Entity.Thumbnail;
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                    }
                }

                IsBusy = false;
            });
        }

        //internal void BeginLoad()
        //{
        //    IsBusy = true;
        //}

        //internal void Loaded()
        //{
        //    IContext context = this.RetrieveContext();

        //    Folders = new ObservableCollection<Folder>(Folder.ReadAll(context).Reverse());

        //    IsBusy = false;
        //}

        [RelayCommand]
        private async Task AddPage(FolderViewModel folder)
        {
            NewPage = new Page();
            AddPageStep = 0;

            await App.Current.MainPage.Navigation.PushAsync(new PageEditorView(new Page(), folder));
        }

        [RelayCommand]
        private async Task CloseFolderAddButtonActionPage()
        {
            await App.Current.MainPage.Navigation.PopModalAsync(true);
        }

        [RelayCommand]
        private async Task ShowPageAddActions()
        {
            await App.Current.MainPage.Navigation.PushModalAsync(new FolderAddButtonActionPage());
        }

        //[RelayCommand]
        //private async Task NextNewFolderStep()
        //{
        //    NewFolderStep++;

        //    if (NewFolderStep > 2)
        //    {
        //        IContext context = this.RetrieveContext();

        //        NewFolder.Thumbnail = NewFolderImage;
        //        NewFolder.Save(context);

        //        NewFolder = null;

        //        await App.Current.MainPage.Navigation.PopAsync();

        //        ServiceProvider.GetService<IDataRequestService>().RequestLoad();

        //        NewFolderStep = 0;
        //    }
        //}

        //[RelayCommand]
        //private async Task NextAddPageStep(Folder folder)
        //{
        //    AddPageStep++;

        //    if (AddPageStep > 1)
        //    {
        //        IContext context = this.RetrieveContext();

        //        SelectedFolder.AddPage(NewPage);
        //        SelectedFolder.Save(context);
        //        await App.Current.MainPage.Navigation.PopAsync();


        //        AddPageStep = 0;
        //        var tmp = SelectedFolder;
        //        SelectedFolder = null;
        //        SelectedFolder = tmp;
        //        Loaded();
        //    }

        //    //// TODO: State enum
        //    //if (AddPageStep == 2)
        //    //{
        //    //    NewPage.Save(this.RetrieveContext());
        //    //    await App.Current.MainPage.Navigation.PopAsync();
        //    //    return;
        //    //}

        //    //await Task.FromResult(false);

        //}

        [RelayCommand]
        private async Task AddPageDocument()
        {
            Document document = new Document();

            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult image = await MediaPicker.Default.CapturePhotoAsync();

                if (image != null)
                {
                    using Stream sourceStream = await image.OpenReadAsync();




                    using MemoryStream byteStream = new();
                    await sourceStream.CopyToAsync(byteStream);
                    byte[] bytes = byteStream.ToArray();

                    string content = Convert.ToBase64String(bytes);

                    document.Image = bytes;

                    NewPage.AddDocument(document);
                }
            }
        }

        [RelayCommand]
        private async Task FolderClicked(FolderViewModel selectedFolder)
        {
            await App.Current.MainPage.Navigation.PushAsync(new FolderDetailsView(selectedFolder));
        }

        [RelayCommand]
        private async Task AddNewFolder()
        {
            NewFolder = new FolderViewModel(new Folder()); 
            await App.Current.MainPage.Navigation.PopModalAsync();
            await App.Current.MainPage.Navigation.PushAsync(new FolderEditorView(null));
        }

        //[RelayCommand]
        //private async Task PageSelected(Page selectedPage)
        //{
        //    SelectedPage = selectedPage;
        //    await App.Current.MainPage.Navigation.PushAsync(new PageDetailsView());
        //}

        [RelayCommand]
        private async Task SaveFolder()
        {
            IContext context = this.RetrieveContext();

            NewFolder.Thumbnail = NewFolderImage;
            NewFolder.Folder.Save(context);
            
            await App.Current.MainPage.Navigation.PopAsync();
            ServiceProvider.GetService<IDataBrokerService>().Publish(context, NewFolder.Folder, UpdateType.Add);
        }

        //[RelayCommand]
        //private async Task NextAddDocumentStep()
        //{
        //    AddDocumentStep++;

        //    if (AddDocumentStep > 2)
        //    {
        //        AddDocumentStep = 0;

        //        NewDocument.Image = NewDocumentImage;
        //        SelectedPage.AddDocument(NewDocument);
        //        SelectedPage.Save(this.RetrieveContext());
        //        ServiceProvider.GetService<IDataBrokerService>().Publish(this.RetrieveContext(), SelectedPage, UpdateType.Add);
        //        await App.Current.MainPage.Navigation.PopAsync();
        //    }
        //}

        //[RelayCommand]
        //private async Task EditFolder(Folder folder)
        //{
        //    NewFolderStep = 0;
        //    NewFolder = folder;
        //    await App.Current.MainPage.Navigation.PushAsync(new FolderEditorView(folder));
        //}


        //[RelayCommand]
        //private async Task TakeDocumentImage()
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

        //            NewDocumentImage = bytes;
        //        }
        //    }
        //}

        //[RelayCommand]
        //private async Task PickDocumentImage()
        //{
        //    var image = await MediaPicker.PickPhotoAsync();

        //    if (image != null)
        //    {
        //        using Stream sourceStream = await image.OpenReadAsync();

        //        using MemoryStream byteStream = new();
        //        await sourceStream.CopyToAsync(byteStream);
        //        byte[] bytes = byteStream.ToArray();

        //        string content = Convert.ToBase64String(bytes);

        //        NewDocumentImage = bytes;
        //    }
        //}

        //[RelayCommand]
        //private async Task AddDocument()
        //{
        //    NewDocument = new Document();
        //    await App.Current.MainPage.Navigation.PushAsync(new DocumentEditorView());
        //}

        //[RelayCommand]
        //private async Task PickFolderThumbnail()
        //{
        //    var image = await MediaPicker.PickPhotoAsync();

        //    if (image != null)
        //    {
        //        using Stream sourceStream = await image.OpenReadAsync();

        //        using MemoryStream byteStream = new();
        //        await sourceStream.CopyToAsync(byteStream);
        //        byte[] bytes = byteStream.ToArray();

        //        string content = Convert.ToBase64String(bytes);

        //        NewFolderImage = bytes;
        //    }
        //}

        //[RelayCommand]
        //private async Task TakeFolderThumbnail()
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

        //            NewFolderImage = bytes;
        //        }
        //    }
        //}

        [RelayCommand]
        private async Task CaptureImage()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                IContext context = this.RetrieveContext();

                context.Log("Taking picture");

                FileResult image = await MediaPicker.Default.CapturePhotoAsync();

                if (image != null)
                {
                    using Stream sourceStream = await image.OpenReadAsync();

                    using MemoryStream byteStream = new();
                    await sourceStream.CopyToAsync(byteStream);
                    byte[] bytes = byteStream.ToArray();

                    string content = Convert.ToBase64String(bytes);

                    Document doc = new Document(new DocumentKey(DocumentKey.TemporaryId));

                    doc.Image = bytes;
                    doc.Name = "test";
                    doc.Comment = " comment";

                    doc.Save(context);

                    ServiceProvider.GetService<IDataBrokerService>().Publish(context, doc, UpdateType.Add);
                }
            }
        }

    }
}
