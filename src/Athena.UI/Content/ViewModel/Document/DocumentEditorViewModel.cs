using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using Com.Spflaum.Documentscanner;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.AdMob.Services;

namespace Athena.UI
{
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

        public Guid Id { get; private set; }

        public DocumentImageViewModel()
        {
            Id = Guid.NewGuid();
        }
    }

    public partial class DocumentEditorViewModel : ContextViewModel
    {
        private readonly IInterstitialAdService _interstitialAdService;
        private readonly ViewStepHandler<DocumentEditorViewModel> _stepHandler;

        [ObservableProperty]
        private ObservableCollection<PdfCreationSummaryStep> _documentReports;

        [ObservableProperty]
        private bool _areDocumentsEmpty;

        [ObservableProperty]
        private ObservableCollection<DocumentImageViewModel> _images;

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
        private string _detectTextInfo;

        [ObservableProperty]
        private bool _detectTextPossible;

        [ObservableProperty]
        private bool _tagsAvailable;

        private DocumentViewModel _document;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _tags;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _selectedTags;

        public Folder ParentFolder => _parentFolder;

        private readonly Folder _parentFolder;

        public DocumentViewModel Document
        {
            get => _document;

            set
            {
                _document = value;
                OnPropertyChanged();
            }
        }

        public ViewStepHandler<DocumentEditorViewModel> StepHandler => _stepHandler;

        public DocumentEditorViewModel(Folder parentFolder, Document document)
        {
            _stepHandler = new(this);
            _stepHandler.RegisterIncrease(1);
            _stepHandler.Register(2, new DocumentCreatePdfStep());
            _stepHandler.Register(3, new DocumentCloseStep());

            DocumentReports = new();
            _interstitialAdService = Services.GetService<IInterstitialAdService>();

            IsNew = document == null;

            document ??= new Document()
            {
                Name = string.Empty
            };

            _parentFolder = parentFolder;

            Document = document;
            Document.PropertyChanged += DocumentOnPropertyChanged;

            Images = new();
            AreDocumentsEmpty = true;

            Tags = new ObservableCollection<TagViewModel>(Tag.ReadAll(RetrieveContext()).OrderBy(x => x.Name).Select(x => new TagViewModel(x)));
            SelectedTags = new();

            TagsAvailable = Tags.Any();

            if (!IsNew)
            {
                _stepHandler.StepIndex = 1;
                AreDocumentsEmpty = false;
                NextStepCommand.NotifyCanExecuteChanged();

                HashSet<int> ids = new HashSet<int>(document.Tags.Select(x => x.Id));

                foreach (TagViewModel tag in Tags)
                {
                    if (ids.Contains(tag.Id))
                        SelectedTags.Add(tag);
                }
            }

            IOcrService ocrService = Services.GetService<IOcrService>();

            if (ocrService.Error != OcrError.None)
            {
                DetectTextInfo = Localization.TextDetectionNotAvailable;
                DetectTextPossible = false;
            }
            else
            {
                DetectTextPossible = true;
            }

            DetectText = DetectTextPossible;
        }

        public void ShowAd()
        {
            _interstitialAdService.ShowAd();
        }

        public async Task PrepareAd()
        {
            PopupText = "Preparing editor...";
            IsPopupOpen = true;


            await Task.Run(() => MainThread.BeginInvokeOnMainThread(() => _interstitialAdService.PrepareAd("ca-app-pub-7134624676592827/8601607180")));
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
            return !AreDocumentsEmpty && (_stepHandler.StepIndex != 1 || !string.IsNullOrWhiteSpace(Document.Name));
        }

        internal async Task CloseAsync()
        {
            await PopModalAsync();
            _stepHandler.StepIndex = 0;
        }

        [RelayCommand(CanExecute = "CanExecuteNextStep")]
        private async Task NextStep()
        {
            try
            {
                if (!IsNew)
                {
                    IContext context = RetrieveContext();

                    List<TagViewModel> newTags
                        = SelectedTags
                            .Where(tag => Document.Tags.All(x => x.Id != tag.Id))
                            .ToList();

                    List<TagViewModel> deletedTags
                        = Document.Tags
                            .Where(tag => SelectedTags.All(x => x.Id != tag.Id))
                            .Select(tag => (TagViewModel)tag)
                            .ToList();

                    foreach (TagViewModel newTag in newTags)
                    {
                        Document.AddTag(context, newTag);
                        //Document.Document.AddTag(context, newTag);
                    }

                    foreach (TagViewModel deletedTag in deletedTags)
                    {
                        Document.DeleteTag(context, deletedTag);
                        //Document.Document.DeleteTag(context, deletedTag);
                    }

                    Document.Document.Save(context);

                    Services.GetService<IDataBrokerService>().Publish<Document>(
                        context,
                        Document,
                        UpdateType.Edit,
                        _parentFolder?.Key);

                    await Toast.Make("Successfully updated document " + Document.Name).Show();

                    await CloseAsync();
                    return;
                }

                await _stepHandler.Next(RetrieveContext());
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
                    IContext context = RetrieveContext();

                    context.Log("Taking picture");

                    FileResult image = await MediaPicker.Default.CapturePhotoAsync();

                    if (image != null)
                    {
                        await using Stream sourceStream = await image.OpenReadAsync();

                        using MemoryStream byteStream = new();
                        await sourceStream.CopyToAsync(byteStream);
                        byte[] bytes = byteStream.ToArray();

                        // https://github.com/dotnet/maui/issues/11259

                        IsPopupOpen = true;
                        PopupText = "Loading cropping tool...";

                        await Task.Delay(100);

                        DocumentEditorDocumentCropView view = new DocumentEditorDocumentCropView(bytes);

                        IsPopupOpen = false;

                        await PushModalAsync(view);
                        view.ImageSaved += OnImageSaved;
                    }
                }
            }
            catch (Exception ex)
            {
                IContext context = RetrieveContext();
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

                FileResult result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    await using (Stream stream = await result.OpenReadAsync())
                    {
                        using MemoryStream byteStream = new();
                        await stream.CopyToAsync(byteStream);

                        DocumentImageViewModel imageVm = new DocumentImageViewModel
                        {
                            IsPdf = true, ImagePath = result.FullPath, FileName = result.FileName
                        };

                        Images.Add(imageVm);

                        AreDocumentsEmpty = false;
                    }
                }

            }
            catch (Exception ex)
            {
                IContext context = RetrieveContext();
                context.Log(ex);
                await Toast.Make("An error occurred").Show();
            }
        }

        [RelayCommand]
        private async Task PickImage()
        {
            try
            {
                FileResult image = await MediaPicker.PickPhotoAsync();

                if (image != null)
                {
                    await using Stream sourceStream = await image.OpenReadAsync();

                    using MemoryStream byteStream = new();
                    await sourceStream.CopyToAsync(byteStream);
                    byte[] bytes = byteStream.ToArray();

                    // https://github.com/dotnet/maui/issues/11259

                    IsPopupOpen = true;
                    PopupText = "Loading cropping tool...";

                    await Task.Delay(100);

                    DocumentScannerWrapper scanner = new DocumentScannerWrapper(Platform.CurrentActivity);
                    scanner.LaunchScanner(1234);
                    //DocumentEditorDocumentCropView view = new DocumentEditorDocumentCropView(bytes);

                    IsPopupOpen = false;

                    //await PushModalAsync(view);
                    //view.ImageSaved += OnImageSaved;

                }
            }
            catch (Exception ex)
            {
                IContext context = RetrieveContext();
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

            DocumentImageViewModel imageVm = new DocumentImageViewModel { Image = e.Buffer };

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Guid.NewGuid() + ".jpg");
            await File.WriteAllBytesAsync(fileName, e.Buffer);

            imageVm.ImagePath = fileName;
            Images.Add(imageVm);
            AreDocumentsEmpty = false;
            await PopModalAsync();
        }

        [RelayCommand]
        private void DeleteImage(object image)
        {
            if (image is not DocumentImageViewModel imageVm)
                return;

            Images.Remove(imageVm);
            AreDocumentsEmpty = Images.Count == 0;
        }

        public async Task BackButton()
        {
            if (!IsNew)
                await PopModalAsync();

            if (!await _stepHandler.Back(RetrieveContext()))
                await PopModalAsync();
        }
    }
}
