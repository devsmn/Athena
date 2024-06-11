using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics.Platform;
using Plugin.AdMob.Services;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using TesseractOcrMaui.Results;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Mvvm.Input;
    using System.IO.Compression;
    using TesseractOcrMaui;

    public partial class DocumentImageViewModel : ObservableObject
    {
        [ObservableProperty]
        private byte[] _image;

        [ObservableProperty]
        private string _imagePath;

        [ObservableProperty]
        private bool _isPdf;

        [ObservableProperty]
        private string _fileName;

        public DocumentImageViewModel()
        {
        }

    }

    public partial class DocumentEditorViewModel : ContextViewModel
    {
        private readonly IInterstitialAdService _interstitialAdService;


        [ObservableProperty]
        private bool _areDocumentsEmpty;

        [ObservableProperty]
        private ObservableCollection<DocumentImageViewModel> _images;

        [ObservableProperty]
        private int _documentStep;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _popupText;

        [ObservableProperty]
        private bool _isPopupOpen;

        [ObservableProperty]
        private string _busyText;

        [ObservableProperty]
        private bool _detectText;

        [ObservableProperty]
        private bool _tagsAvailable;

        private DocumentViewModel _document;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _tags;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _selectedTags;

        private readonly Page _page;
        private readonly Folder _folder;

        public DocumentViewModel Document
        {
            get { return _document; }

            set
            {
                _document = value;
                OnPropertyChanged();
            }
        }

        public DocumentEditorViewModel(Folder folder, Page page, Document document)
        {
            _interstitialAdService = ServiceProvider.GetService<IInterstitialAdService>();
            //_interstitialAdService.PrepareAd();

            IsNew = document == null;

            document ??= new Document()
            {
                Name = string.Empty
            };

            _folder = folder;
            _page = page;
            Document = document;
            Document.PropertyChanged += DocumentOnPropertyChanged;

            Images = new();
            AreDocumentsEmpty = true;

            Tags = new ObservableCollection<TagViewModel>(Tag.ReadAll(RetrieveContext()).OrderBy(x => x.Name).Select(x => new TagViewModel(x)));
            SelectedTags = new();

            TagsAvailable = Tags.Any();

            if (!IsNew)
            {
                DocumentStep = 1;
                AreDocumentsEmpty = false;
                NextStepCommand.NotifyCanExecuteChanged();
            }
        }

        public async Task PrepareAd()
        {
            PopupText = "Preparing editor...";
            IsPopupOpen = true;


            await Task.Run(() => MainThread.BeginInvokeOnMainThread(() => _interstitialAdService.PrepareAd()));
            IsPopupOpen = false;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(AreDocumentsEmpty))
            {
                NextStepCommand.NotifyCanExecuteChanged();
            }
        }

        private void DocumentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Document.Name))
            {
                NextStepCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanExecuteNextStep()
        {
            return !AreDocumentsEmpty && (DocumentStep != 1 || !string.IsNullOrWhiteSpace(Document.Name));
        }

        private async Task CloseAsync()
        {
            await PopModalAsync();
            DocumentStep = 0;
        }

        [RelayCommand(CanExecute = "CanExecuteNextStep")]
        private async Task NextStep()
        {
            try
            {
                if (!IsNew)
                {
                    // Save
                    var context = RetrieveContext();
                    Document.Document.Save(context);
                    ServiceProvider.GetService<IDataBrokerService>().Publish<Document>(context, Document, UpdateType.Edit, _page.Key);

                    await Toast.Make("Successfully updated document " + Document.Name).Show();

                    await CloseAsync();
                    return;
                }

                if (DocumentStep == 2)
                {
                    await CloseAsync();
                    return;
                }

                DocumentStep++;


                if (DocumentStep > 1)
                {
                    _interstitialAdService.ShowAd();
                    var context = RetrieveContext();


                    IsBusy = true;
                    BusyText = "Converting document to pdf...";

                    await Task.Run(async () =>
                    {
                        //Create a new PDF document
                        PdfDocument doc = new PdfDocument();
                        doc.Compression = PdfCompressionLevel.Best;
                        doc.DocumentInformation.Author = "Athena: Offline Document Manager";
                        doc.DocumentInformation.CreationDate = DateTime.UtcNow;
                        doc.DocumentInformation.Title = "Athena: Offline Document Manager - " + Document.Name;
                        doc.DocumentInformation.Subject = "Athena: Offline Document Manager - " + Document.Name;
                        doc.DocumentInformation.Producer = "Athena: Offline Document Manager";
                        doc.DocumentInformation.Keywords = "Athena: Offline Document Manager;PDF";

                        int docIdx = 0;
                        StringBuilder pdfDocs = new StringBuilder();

                        foreach (var document in Images)
                        {
                            MainThread.BeginInvokeOnMainThread(() => BusyText = $"Converting document #{++docIdx}");

                            if (string.IsNullOrWhiteSpace(document.ImagePath))
                            {
                                continue;
                            }

                            if (document.IsPdf)
                            {
                                MainThread.BeginInvokeOnMainThread(() => BusyText = $"Loading pdf {document.FileName}");
                                FileStream pdfStream = new FileStream(document.ImagePath, FileMode.Open, FileAccess.Read);
                                PdfLoadedDocument loadedDoc = new PdfLoadedDocument(pdfStream);

                                MainThread.BeginInvokeOnMainThread(() => BusyText = $"Merging pdf {document.FileName}");
                                PdfDocumentBase.Merge(doc, loadedDoc);

                                if (DetectText)
                                {
                                    MainThread.BeginInvokeOnMainThread(() => BusyText = $"Extracting text from pdf {document.FileName}");
                                    foreach (PdfLoadedPage loadedPage in loadedDoc.Pages)
                                    {
                                        pdfDocs.AppendLine(loadedPage.ExtractText(true));
                                    }
                                }

                                continue;
                            }

                            FileStream imageStream = new FileStream(document.ImagePath, FileMode.Open, FileAccess.Read);

                            Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(imageStream).Downsize(1080, true);
                            MemoryStream ms = new MemoryStream();

                            await img.SaveAsync(ms);

                            PdfBitmap image = new PdfBitmap(ms);
                            PdfUnitConverter converter = new PdfUnitConverter();
                            var size = converter.ConvertFromPixels(image.PhysicalDimension, PdfGraphicsUnit.Pixel);
                            PdfSection section = doc.Sections.Add();

                            section.PageSettings.Size = size;
                            section.PageSettings.Margins.All = 0;
                            var page = section.Pages.Add();

                            page.Graphics.DrawImage(image, 0, 0, size.Width, size.Height);
                        }

                        //Creating the stream object
                        MemoryStream preCompressStream = new MemoryStream();

                        MainThread.BeginInvokeOnMainThread(() => BusyText = "Saving pdf");

                        //Save the document as stream
                        doc.Save(preCompressStream);
                        //If the position is not set to '0' then the PDF will be empty
                        preCompressStream.Position = 0;
                        //Close the document
                        doc.Close(true);

                        MainThread.BeginInvokeOnMainThread(() => BusyText = "Saving document");

                        Document.Pdf = preCompressStream.ToArray();
                        preCompressStream.Close();
                        await preCompressStream.DisposeAsync();

                        MainThread.BeginInvokeOnMainThread(() => BusyText = "Saving tags...");

                        foreach (var tag in _selectedTags)
                        {
                            Document.Document.Tags.Add(tag);
                        }

                        _page.AddDocument(Document);
                        _page.Save(context);

                        if (DetectText)
                        {
                            var ocr = ServiceProvider.GetService<ITesseract>();

                            docIdx = 0;

                            StringBuilder sb = new StringBuilder();

                            foreach (var document in Images)
                            {
                                if (document.IsPdf)
                                    continue;

                                MainThread.BeginInvokeOnMainThread(() => BusyText = $"Detecting text in document #{++docIdx}. This may take a while");


                                var result = await ocr.RecognizeTextAsync(document.ImagePath);
                                if (!result.FinishedWithSuccess())
                                {
                                    //await App.Current.MainPage.DisplayAlert(
                                    //    "Recognize",
                                    //    $"Unable to recognize any text{Environment.NewLine}Message={result.Message}{Environment.NewLine}Status={result.Status}",
                                    //    "Ok");
                                }
                                else
                                {
                                    sb.Append(result.RecognisedText);
                                    sb.AppendLine();
                                }
                            }

                            sb.Append(pdfDocs);

                            if (sb.Length > 0)
                            {

                                Chapter chapter = new Chapter(Document.Document.Key.Id, docIdx, sb.ToString())
                                {
                                    FolderId = _folder.Id.ToString(),
                                    PageId = _page.Id.ToString()
                                };

                                chapter.Save(context);
                            }

                        }
                    });

                    BusyText = "Publishing changes...";

                    ServiceProvider.GetService<IDataBrokerService>()
                        .Publish(context, Document.Document, UpdateType.Add, _page.Key);

                    ServiceProvider.GetService<IDataBrokerService>()
                        .Publish(context, _page, UpdateType.Edit, _folder.Key);

                    IsBusy = false; 
                    BusyText = "Finished!";
                }
            }
            catch (Exception ex)
            {
                await Toast.Make("An error occurred").Show();
                Debug.WriteLine(ex);
                await PopModalAsync();
            }
        }

        [RelayCommand]
        private async Task TakeImage()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var context = RetrieveContext();

                    context.Log("Taking picture");

                    FileResult image = await MediaPicker.Default.CapturePhotoAsync();

                    if (image != null)
                    {
                        using Stream sourceStream = await image.OpenReadAsync();

                        using MemoryStream byteStream = new();
                        await sourceStream.CopyToAsync(byteStream);
                        byte[] bytes = byteStream.ToArray();

                        // https://github.com/dotnet/maui/issues/11259

                        IsPopupOpen = true;
                        PopupText = "Loading cropping tool...";

                        await Task.Delay(100);

                        var view = new DocumentEditorDocumentCropView(bytes);

                        IsPopupOpen = false;

                        await PushModalAsync(view);
                        view.ImageSaved += OnImageSaved;
                    }
                }
            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);

                if (ex is PermissionException pex)
                {
                    await Toast.Make("Please grant the permission to use the camera").Show();
                }
                else
                {
                    await Toast.Make("An error occurred").Show();
                }
            }
        }

        [RelayCommand]
        private async Task PickFile()
        {
            try
            {
                PickOptions options = new()
                {
                    FileTypes = FilePickerFileType.Pdf
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    using (var stream = await result.OpenReadAsync())
                    {
                        using MemoryStream byteStream = new();
                        await stream.CopyToAsync(byteStream);

                        DocumentImageViewModel imageVm = new DocumentImageViewModel();
                        imageVm.IsPdf = true;
                        imageVm.ImagePath = result.FullPath;
                        imageVm.FileName = result.FileName;

                        Images.Add(imageVm);

                        AreDocumentsEmpty = false;
                    }
                }

            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);
                await Toast.Make("An error occurred").Show();
            }
        }

        [RelayCommand]
        private async Task PickImage()
        {
            try
            {
                var image = await MediaPicker.PickPhotoAsync();

                if (image != null)
                {
                    using Stream sourceStream = await image.OpenReadAsync();

                    using MemoryStream byteStream = new();
                    await sourceStream.CopyToAsync(byteStream);
                    byte[] bytes = byteStream.ToArray();

                    // https://github.com/dotnet/maui/issues/11259

                    IsPopupOpen = true;
                    PopupText = "Loading cropping tool...";

                    await Task.Delay(100);


                    var view = new DocumentEditorDocumentCropView(bytes);

                    IsPopupOpen = false;

                    await PushModalAsync(view);
                    view.ImageSaved += OnImageSaved;

                }
            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);
                await Toast.Make("An error occurred").Show();
            }
        }

        private async void OnImageSaved(object sender, ImageSavedEventArgs e)
        {
            if (Document.Thumbnail == null || Document.Thumbnail.Length == 0)
            {
                Document.Thumbnail = e.Buffer;
            }

            DocumentImageViewModel imageVm = new DocumentImageViewModel();
            imageVm.Image = e.Buffer;

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Guid.NewGuid() + ".jpg");//address
            await File.WriteAllBytesAsync(fileName, e.Buffer);

            imageVm.ImagePath = fileName;
            Images.Add(imageVm);
            AreDocumentsEmpty = false;
            await PopModalAsync();
        }

        [RelayCommand]
        private void DeleteImage(DocumentImageViewModel image)
        {
            Images.Remove(image);
            AreDocumentsEmpty = Images.Count == 0;
        }

    }
}
