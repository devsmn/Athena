namespace Athena.UI
{
    using DataModel;

    public partial class FolderEditorView : DefaultContentPage
    {
        public FolderEditorView(RootItemViewModel folderToEdit)
        {
            BindingContext = new FolderEditorViewModel(folderToEdit, DoneTcs);
            InitializeComponent();
        }
    }
}
