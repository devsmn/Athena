using System.ComponentModel;
using Syncfusion.Maui.ImageEditor;

namespace Athena.UI
{

    public partial class DocumentEditorDocumentCropView : ContentPage
    {
        private readonly string[] _hideItems = new[] {
            "Text", "Add", "TextColor", "FontFamily", "Arial", "Noteworthy", "Marker Felt", "Bradley Hand",
            "SignPainter",
            "TextEffects", "Bold", "Italic", "Underline", "Opacity", "Path", "StrokeThickness", "Colors", "Shape",
            "Rectangle",
            "Circle", "Arrow", "Effects", "Hue", "Saturation", "Brightness", "Contrast", "Blur", "Sharpen", "Pen", "Browse"
        };

        public DocumentEditorDocumentCropView(byte[] image)
        {
            var vm = new DocumentEditCropViewModel(image);

            vm.ImageSaved += (s, e) =>
            {
                ImageSaved?.Invoke(s, e);
            };

            BindingContext = vm;

            InitializeComponent();
            HideToolbarItems();
        }

        private void HideToolbarItems()
        {
            foreach (var item in _hideItems)
            {
                ImageEditor.SetToolbarItemVisibility(item, false);
            }
        }


        public EventHandler<ImageSavedEventArgs> ImageSaved;


        private void OnSavePickerOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private byte[] GetImageStreamAsBytes(Stream input)
        {
            var buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }

        }

        private async void ImageEditor_OnToolbarItemSelected(object sender, ToolbarItemSelectedEventArgs e)
        {
            if (e.ToolbarItem != null && e.ToolbarItem.Name == "Save")
            {
                e.Cancel = true;
                var imageStream = await ImageEditor.GetImageStream();
                var bytes = GetImageStreamAsBytes(imageStream);

                (BindingContext as DocumentEditCropViewModel).SavingImageCommand(new ImageSavedEventArgs(bytes));
            }
        }
    }
}