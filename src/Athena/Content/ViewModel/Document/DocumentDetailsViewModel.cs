using System.Collections.ObjectModel;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#if ANDROID
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Maui.Alerts;

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

        private readonly Folder _parentFolder;

        public DocumentDetailsViewModel(Folder parentFolder, Document document)
        {
            Document = document;
            _parentFolder = parentFolder;

            AllTags = new (Tag.ReadAll(RetrieveContext()).Select(x => new TagViewModel(x)));
            SelectedTags = new ObservableCollection<TagViewModel>();

            foreach (var tag in AllTags)
            {
                if (document.Tags.Any(x => x.Id == tag.Id))
                    SelectedTags.Add(tag);
            }
        }

        public DocumentDetailsViewModel(Chapter chapter)
        {
            Document = chapter.Document;
            _chapter = chapter;

            IsSearchResult = true;
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
            ServiceProvider.GetService<IDataBrokerService>().Publish<Document>(context, Document, UpdateType.Delete, _parentFolder?.Key);

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
                string mainDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);

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

                ServiceProvider.GetService<IDataBrokerService>().Publish(
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
