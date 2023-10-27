using Athena.DataModel;

namespace Athena.UI;

public partial class FolderDetailsView : ContentPage
{
	public FolderDetailsView(Folder selectedFolder)
    {
        this.BindingContext = new FolderDetailsViewModel(selectedFolder);
        InitializeComponent();
    }
}