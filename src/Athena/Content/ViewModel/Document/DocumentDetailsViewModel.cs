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
        private readonly Page _page;
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
        private ObservableCollection<TagViewModel> _allTags;

        [ObservableProperty]
        private ObservableCollection<TagViewModel> _selectedTags;

        public DocumentDetailsViewModel(Page page, Document document)
        {
            _page = page;
            Document = document;

            AllTags = new ObservableCollection<TagViewModel>(Tag.ReadAll(this.RetrieveContext()).Select(x => new TagViewModel(x)));
            SelectedTags = new ObservableCollection<TagViewModel>();

            foreach (var tag in AllTags)
            {
                if (document.Tags.Any(x => x.Id == tag.Id))
                    SelectedTags.Add(tag);
            }
        }

        public DocumentDetailsViewModel(Page page, Chapter chapter)
        {
            _page = page;
            Document = chapter.Document;
            _chapter = chapter;

            this.IsSearchResult = true;
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
                    if (update.Type == UpdateType.Add)
                    {
                        AllTags.Add(update.Entity);
                    }
                    else if (update.Type == UpdateType.Remove)
                    {
                        AllTags.Remove(update.Entity);
                    }
                    else if (update.Type == UpdateType.Edit)
                    {
                        var tagToEdit = AllTags.FirstOrDefault(x => x.Id == update.Entity.Id);

                        if (tagToEdit != null)
                            tagToEdit.Name = update.Entity.Name;
                    }
                }
            }
        }

        [RelayCommand]
        private async Task SaveTags()
        {
            var context = this.RetrieveContext();

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

            var context = this.RetrieveContext();

            Document.Document.Delete(context);

            await Toast.Make(string.Format(Localization.DocumentDeleted, Document.Name), ToastDuration.Long).Show();

            ServiceProvider.GetService<IDataBrokerService>().Publish<Document>(context, Document, UpdateType.Remove, _page.Key);
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
                var context = this.RetrieveContext();
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
            await PushAsync(new DocumentDetailsPdfView(Document.Pdf,
                Convert.ToInt32(_chapter.DocumentPageNumber)));
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
                var context = this.RetrieveContext();
                context.Log(ex);
            }

        }

        [RelayCommand]
        private async Task DeleteDocument()
        {
            ShowMenuPopup = false;

            var result = await DisplayAlert(
                Localization.DeleteDocument,
                string.Format(Localization.DeleteDocumentConfirm, Document.Name),
                Localization.Yes,
                Localization.No);

            if (result)
            {
                var context = this.RetrieveContext();
                this.Document.Document.Delete(context);

                await Toast.Make(string.Format(Localization.DocumentDeleted, Document.Name), ToastDuration.Long).Show();

                ServiceProvider.GetService<IDataBrokerService>().Publish(
                    context,
                    this.Document.Document,
                    UpdateType.Remove,
                    this._page.Key);

                await PopAsync();
            }
        }
        

        [RelayCommand]
        private async Task EditDocument()
        {
            await PushModalAsync(new DocumentEditorView(null, this._page, this.Document));
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
                var context = this.RetrieveContext();
                context.Log(ex);
            }
        }
    }
}
