using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Core;

    public partial class FolderDetailsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private FolderViewModel _folder;

        [ObservableProperty]
        private bool _showInfoPopup;

        [ObservableProperty]
        private bool _showMenuPopup;

        [ObservableProperty]
        private bool _showPageMenuPopup;

        [ObservableProperty]
        private PageViewModel _selectedPage;

        private readonly Folder _dummyFolder;

        [ObservableProperty]
        private bool _isBusy;

        public FolderDetailsViewModel(Folder folder)
        {
            _dummyFolder = folder;
        }
        
        internal void LoadPages()
        {
            IsBusy = true;

            Task.Run(() =>
            {
                _ = _dummyFolder.Pages.Count;

                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    this.Folder = new FolderViewModel(_dummyFolder);
                    IsBusy = false;
                });
            });
        }

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            var folderUpdate = e.Folders.FirstOrDefault(x => x.Entity == this.Folder.Folder);

            if (folderUpdate != null)
            {
                if (folderUpdate.Type == UpdateType.Edit)
                {
                    this.Folder.Comment = folderUpdate.Entity.Comment;
                    this.Folder.Name = folderUpdate.Entity.Name;
                }
            }

            foreach (var pageUpdate in e.Pages.Where(x => x.ParentReference == this.Folder.Folder.Key))
            {
                if (pageUpdate.Type == UpdateType.Add)
                {
                    this.Folder.AddPage(pageUpdate.Entity);
                }
                else if (pageUpdate.Type == UpdateType.Edit)
                {
                    var page = this.Folder.Pages.FirstOrDefault(x => x.Page == pageUpdate.Entity);

                    if (page != null)
                    {
                        page.Comment = pageUpdate.Entity.Comment;
                        page.Title = pageUpdate.Entity.Title;
                    }
                }
                else if (pageUpdate.Type == UpdateType.Remove)
                {
                    var removedPage = this.Folder.Pages.FirstOrDefault(x => x.Page == pageUpdate.Entity);

                    if (removedPage != null)
                    {
                        this.Folder.RemovePage(removedPage);
                    }
                }
            }
            
        }

        [RelayCommand]
        private void InfoClicked()
        {
            ShowMenuPopup = false;
            ShowInfoPopup = true;
        }

        [RelayCommand]
        private async Task DeleteClicked()
        {
            ShowMenuPopup = false;

            bool result = await DisplayAlert(
                Localization.DeleteFolder,
                string.Format(Localization.DeleteFolderConfirm, Folder.Name),
                Localization.Yes,
                Localization.No);
            
            if (!result)
                return;

            var context = this.RetrieveContext();
            Folder.Folder.Delete(context);
            ServiceProvider.GetService<IDataBrokerService>().Publish<Folder>(context, this.Folder, UpdateType.Remove);
            await Toast.Make(string.Format(Localization.FolderDeleted, Folder.Name), ToastDuration.Long).Show();
            await PopAsync();
        }

        [RelayCommand]
        private async Task AddPage(FolderViewModel selectedFolder)
        {
            await PushAsync(new PageEditorView(new Page(), selectedFolder));
        }

        [RelayCommand]
        private async Task EditFolder()
        {
            await PushAsync(new FolderEditorView(Folder));
        }
        
        [RelayCommand]
        public async Task EditPage(PageViewModel selectedPage)
        {
            ShowPageMenuPopup = false;
            await PushAsync(new PageEditorView(selectedPage, Folder));
        }

        [RelayCommand]
        public async Task DeletePage(PageViewModel selectedPage)
        {
            ShowPageMenuPopup = false;

            bool result = await DisplayAlert(
                Localization.DeletePage,
                string.Format(Localization.DeletePageConfirm, SelectedPage.Title),
                Localization.Yes,
                Localization.No);

            if (!result)
                return;

            var context = this.RetrieveContext();

            selectedPage.Page.Delete(context);

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, selectedPage.Page, UpdateType.Remove, Folder.Folder.Key);

            var page = Folder.Pages.FirstOrDefault(x => x == selectedPage);

            if (page != null)
            {
                Folder.RemovePage(page);
            }

            await Toast.Make(string.Format(Localization.PageDeleted, SelectedPage.Title), ToastDuration.Long).Show();

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, UpdateType.Edit);
        }

        [RelayCommand]
        private async Task PageSelected()
        {
            await PushAsync(new PageDetailsView(Folder, SelectedPage));
            SelectedPage = null;
        }

    }
}
