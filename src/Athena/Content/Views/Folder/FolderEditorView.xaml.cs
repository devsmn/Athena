namespace Athena.UI
{
    using Athena.DataModel;


    public partial class FolderEditorView : ContentPage
    {
        public FolderEditorView(Folder folderToEdit, Folder parentFolder)
        {
            this.BindingContext = new FolderEditorViewModel(folderToEdit, parentFolder);
            InitializeComponent();
        }
    }
}
