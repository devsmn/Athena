using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;

    internal partial class FolderEditorViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private FolderViewModel _folder;

        [ObservableProperty]
        private int _newFolderStep;

        public FolderEditorViewModel(Folder folder)
        {
            if (folder == null)
            {
                folder = new Folder();
                IsNew = true;
            }

            Folder = folder;
        }

        [RelayCommand]
        private async Task NextStep()
        {
            NewFolderStep++;

            if (NewFolderStep > 2)
            {
                IContext context = this.RetrieveContext();
                
                Folder.Folder.Save(context);
                ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, IsNew ? UpdateType.Add : UpdateType.Edit);
                
                await App.Current.MainPage.Navigation.PopAsync();
                NewFolderStep = 0;
            }
        }

        [RelayCommand]
        private async Task PickThumbnail()
        {
            var image = await MediaPicker.PickPhotoAsync();

            if (image != null)
            {
                using Stream sourceStream = await image.OpenReadAsync();

                using MemoryStream byteStream = new();
                await sourceStream.CopyToAsync(byteStream);
                byte[] bytes = byteStream.ToArray();

                string content = Convert.ToBase64String(bytes);

                Folder.Thumbnail = bytes;
            }
        }

        [RelayCommand]
        private async Task TakeThumbnail()
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

                    Folder.Thumbnail = bytes;
                }
            }
        }
    }
}
