namespace Athena.UI
{
    using DataModel;

    public partial class FolderEditorView : ContentPage
    {
        public FolderEditorView(Folder folderToEdit, Folder parentFolder)
        {
            BindingContext = new FolderEditorViewModel(folderToEdit, parentFolder);
            InitializeComponent();
        }
    }
}
