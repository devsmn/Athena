using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.UI;
using Athena.DataModel.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class FolderDetailsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private FolderViewModel _folder;

        public FolderDetailsViewModel(Folder folder)
        {
            this.Folder = new FolderViewModel(folder);

            ServiceProvider.GetService<IDataBrokerService>().Published += OnDataBrokerPublished;
        }

        private void OnDataBrokerPublished(object sender, DataPublishedEventArgs e)
        {
            var folderUpdate = e.Folders.FirstOrDefault(x => x.Entity == this.Folder.Folder);

            if (folderUpdate != null)
            {
                if (folderUpdate.Type == UpdateType.Edit)
                {
                    this.Folder.Comment = folderUpdate.Entity.Comment;
                    this.Folder.Name = folderUpdate.Entity.Name;
                    this.Folder.Thumbnail = folderUpdate.Entity.Thumbnail;
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
                        page.DocumentCount = pageUpdate.Entity.Documents.Count;
                    }
                }
            }
        }

        [RelayCommand]
        private async Task AddPage(FolderViewModel selectedFolder)
        {
            await App.Current.MainPage.Navigation.PushAsync(new PageEditorView(new Page(), selectedFolder));
        }

        [RelayCommand]
        private async Task EditFolder(FolderViewModel selectedFolder)
        {
            await App.Current.MainPage.Navigation.PushAsync(new FolderEditorView(selectedFolder));
        }

        [RelayCommand]
        private async Task DeletePage(PageViewModel selectedPage)
        {
            bool result = await App.Current.MainPage.DisplayAlert(
                "Confirm deletion",
                $"Do you really want to delete the page {selectedPage.Title}?" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"This action cannot be undone!",
                "Yes",
                "No");

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

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, UpdateType.Edit);
        }

        [RelayCommand]
        private async Task PageSelected(PageViewModel selectedPage)
        {
            await App.Current.MainPage.Navigation.PushAsync(new PageDetailsView(Folder, selectedPage));
        }

    }
}
