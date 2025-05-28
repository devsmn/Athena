using Athena.Resources.Localization;

namespace Athena.UI;


public partial class DocumentEditorMetaInfoStep : ContentView
{
    public DocumentEditorMetaInfoStep()
    {
        InitializeComponent();
    }

    private void TagsInfoClicked(object sender, EventArgs e)
    {
        DefaultContentPage page = this.GetParent<DefaultContentPage>();

        if (page == null)
            return;

        page.ShowInfoPopup(Localization.Tags, Localization.DocumentEditorTagsInfo);
    }

}
