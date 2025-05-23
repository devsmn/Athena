using SelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;

namespace Athena.UI;


public partial class TagsOverview : DefaultContentPage
{
    public TagsOverview()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClicked(object sender, EventArgs e)
    {
    }

    private void Group_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }
}
