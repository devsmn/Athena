using Android.Net.Wifi.Aware;

namespace Athena.UI
{
    using DataModel;

    public partial class FolderEditorView : ContentPage
    {
        public const string Route = "folder/edit";

        public FolderEditorView(Folder folderToEdit, Folder parentFolder)
        {
            BindingContext = new FolderEditorViewModel(folderToEdit, parentFolder);
            InitializeComponent();
        }
    }
}
