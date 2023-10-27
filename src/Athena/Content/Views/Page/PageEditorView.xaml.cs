using Athena.DataModel;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class PageEditorView : ContentPage
    {
        public PageEditorView(Page page, Folder folder)
        {
            this.BindingContext = new PageEditorViewModel(page, folder);
            InitializeComponent();
        }
    }
}