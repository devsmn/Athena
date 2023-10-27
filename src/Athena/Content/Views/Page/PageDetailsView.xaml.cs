using System.Runtime.InteropServices.Marshalling;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class PageDetailsView : ContentPage
    {
        public PageDetailsView(Folder folder, Page page)
        {
            this.BindingContext = new PageDetailsViewModel(folder, page);
            InitializeComponent();
        }
    }
}
