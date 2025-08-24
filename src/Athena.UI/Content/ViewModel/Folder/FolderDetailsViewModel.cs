using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
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
        private bool _isBusy;

        protected override void OnDataPublished(DataPublishedArgs data)
        {
            RequestUpdate<Folder> folderUpdate = data.Folders.FirstOrDefault(x => x.Entity == Folder.Folder);

            if (folderUpdate == null)
                return;

            if (folderUpdate.Type == UpdateType.Edit)
            {
                Folder.Comment = folderUpdate.Entity.Comment;
                Folder.Name = folderUpdate.Entity.Name;
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

            IContext context = RetrieveContext();
            Folder.Folder.Delete(context);
            Services.GetService<IDataBrokerService>().Publish<Folder>(context, Folder, UpdateType.Delete);
            await Toast.Make(string.Format(Localization.FolderDeleted, Folder.Name), ToastDuration.Long).Show();
            await PopAsync();
        }
    }
}
