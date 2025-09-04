using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public class ImageSavedEventArgs : EventArgs
    {
        public byte[] Buffer { get; private set; }

        public ImageSavedEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }
    }

    public partial class DocumentEditCropViewModel : ObservableObject
    {
        public EventHandler<ImageSavedEventArgs> ImageSaved;

        [ObservableProperty]
        private byte[] _image;

        public DocumentEditCropViewModel(byte[] image)
        {
            Image = image;
        }

        [RelayCommand]
        public void SavingImageCommand(ImageSavedEventArgs e)
        {
            ImageSaved?.Invoke(this, e);
        }
    }
}
