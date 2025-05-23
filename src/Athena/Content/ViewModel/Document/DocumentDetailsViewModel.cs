using System.Collections.ObjectModel;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace Athena.UI
{
    using DataModel;
    using CommunityToolkit.Maui.Alerts;
    using Athena.DataModel.Core;
    using AndroidX.Startup;

    public partial class DocumentDetailsViewModel : ContextViewModel
    {
        private readonly Chapter _chapter;

        private const string DocumentPrefix = "Athena_AI_Document_Manager_";

        [ObservableProperty]
        private bool _showInfoPopup;

        [ObservableProperty]
        private DocumentViewModel _document;

        [ObservableProperty]
        private bool _showMenuPopup;

        [ObservableProperty]
        private bool _isSearchResult;

        [ObservableProperty]
        private bool _showTagPopup;

        [ObservableProperty]
        private VisualCollection<TagViewModel, Tag> _allTags;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _selectedTags;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private byte[] _pdf;

        private bool _initialized;

        private readonly Folder _parentFolder;

        public DocumentDetailsViewModel(Folder parentFolder, Document document)
        {
            Document = document;
            _parentFolder = parentFolder;
            SelectedTags = new ObservableCollection<TagViewModel>();

        }

        public DocumentDetailsViewModel(Chapter chapter)
        {
            Document = chapter.Document;
            _chapter = chapter;

            IsSearchResult = true;
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsyncBackgroundAction(async context =>
            {
                byte[] pdf = Document.Pdf;

                var allTags = new List<TagViewModel>(Tag.ReadAll(context).Select(x => new TagViewModel(x)));
                var selectedTags = new List<TagViewModel>();

                foreach (var tag in allTags)
                {
                    if (Document.Tags.Any(x => x.Id == tag.Id))
                        selectedTags.Add(tag);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AllTags = new(allTags);
                    SelectedTags = new(selectedTags);
                    Pdf = pdf;
                });
            });
        }

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            if (e.Documents.Any())
            {
                var updateDoc = e.Documents.FirstOrDefault(x => x.Entity.Id == Document.Document.Id);

                if (updateDoc != null)
                {
                    if (updateDoc.Type == UpdateType.Edit)
                    {
                        Document.Name = updateDoc.Entity.Name;
                    }
                }
            }

            if (e.Tags.Any())
            {
                foreach (var update in e.Tags)
                {
                    AllTags.Process(update);
                }
            }
        }

        [RelayCommand]
        private async Task SaveTags()
        {
            var context = RetrieveContext();

            var newTags = SelectedTags.Where(x => !Document.Tags.Any(z => z.Id == x.Id));

            foreach (var tag in newTags)
            {
                Document.Document.Tags.Add(tag);
                Document.Document.AddTag(context, tag);
            }

            var deletedTags = Document.Tags.Where(x => !SelectedTags.Any(z => z.Id == x.Id)).ToList();

            foreach (var tag in deletedTags)
            {
                Document.Document.Tags.Remove(tag);
                Document.Document.DeleteTag(context, tag);
            }

            await Toast.Make(Localization.TagsSavedSuccessfully).Show();
        }

        [RelayCommand]
        private async Task DeleteClicked()
        {
            ShowMenuPopup = false;

            bool delete = await DisplayAlert(
                Localization.DeleteDocument,
                string.Format(Localization.DeleteDocumentConfirm, Document.Name),
                Localization.Yes,
                Localization.No);

            if (!delete)
                return;

            var context = RetrieveContext();

            Document.Document.Delete(context);

            await Toast.Make(string.Format(Localization.DocumentDeleted, Document.Name), ToastDuration.Long).Show();
            Services.GetService<IDataBrokerService>().Publish<Document>(context, Document, UpdateType.Delete, _parentFolder?.Key);

            await PopAsync();
        }

        [RelayCommand]
        private async Task OpenDocument()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Document.Pdf))
                {
                    string dir = FileSystem.CacheDirectory;

                    dir = Path.Combine(dir, DocumentPrefix + Document.Name + ".pdf");
                    await File.WriteAllBytesAsync(dir, Document.Pdf);
                    await Launcher.Default.OpenAsync(new OpenFileRequest("Open file", new ReadOnlyFile(dir)));
                }

            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);
            }
        }

        [RelayCommand]
        private async Task Close()
        {
            await PopModalAsync();
        }


        [RelayCommand]
        private async Task OpenSearchResult()
        {
            await PushAsync(
                new DocumentDetailsPdfView(Document.Pdf, Convert.ToInt32(_chapter.DocumentPageNumber)));
        }

        [RelayCommand]
        private async Task ViewDocument()
        {
            await PushAsync(new DocumentDetailsPdfView(Document.Pdf));
        }


        [RelayCommand]
        private void InfoClicked()
        {
            ShowMenuPopup = false;
            ShowInfoPopup = true;
        }

        [RelayCommand]
        private void TagClicked()
        {
            ShowMenuPopup = false;
            ShowTagPopup = true;
        }

        [RelayCommand]
        private async Task SaveDocument()
        {
            try
            {
                string mainDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (!Directory.Exists(mainDir))
                {
                    Directory.CreateDirectory(mainDir);
                }

                mainDir = Path.Combine(mainDir, DocumentPrefix + Document.Name + ".pdf");

                await File.WriteAllBytesAsync(mainDir, Document.Pdf);

                bool openFolder = await DisplayAlert(
                    "File saved",
                    $"The file has been saved successfully to {mainDir}{Environment.NewLine}" +
                    $"Do you want to open the file?",
                    "Yes",
                    "No");

                if (openFolder)
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest("View PDF", new ReadOnlyFile(mainDir)));
                }
            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);
            }

        }

        [RelayCommand]
        private async Task DeleteDocument()
        {
            ShowMenuPopup = false;

            string name = Document.Name;

            var result = await DisplayAlert(
                Localization.DeleteDocument,
                string.Format(Localization.DeleteDocumentConfirm, name),
                Localization.Yes,
                Localization.No);

            if (result)
            {
                var context = RetrieveContext();
                Document.Document.Delete(context);

                await Toast.Make(string.Format(Localization.DocumentDeleted, name), ToastDuration.Long).Show();

                Services.GetService<IDataBrokerService>().Publish(
                    context,
                    Document.Document,
                    UpdateType.Delete,
                    _parentFolder?.Key);

                await PopAsync();
            }
        }


        [RelayCommand]
        private async Task EditDocument()
        {
            await PushModalAsync(new DocumentEditorView(_parentFolder, Document));
            ShowMenuPopup = false;
        }

        [RelayCommand]
        private async Task ShareDocument()
        {
            ShowMenuPopup = false;
            try
            {
                string dir = FileSystem.CacheDirectory;

                dir = Path.Combine(dir, DocumentPrefix + Document.Name + ".pdf");
                await File.WriteAllBytesAsync(dir, Document.Pdf);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Share the document " + Document.Name,
                    File = new ShareFile(dir, "application/pdf"),
                });

            }
            catch (Exception ex)
            {
                var context = RetrieveContext();
                context.Log(ex);
            }
        }
    }
}
