namespace Athena.UI
{
    using Athena.DataModel;

    public partial class FolderEditorView : ContentPage
    {
        public FolderEditorView(Folder folder)
        {
            this.BindingContext = new FolderEditorViewModel(folder);
            InitializeComponent();
        }
    }
}
