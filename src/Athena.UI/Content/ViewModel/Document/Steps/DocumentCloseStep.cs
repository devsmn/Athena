using Athena.DataModel.Core;

namespace Athena.UI
{
    /// <summary>
    /// Implements the close step when editing a document.
    /// </summary>
    internal class DocumentCloseStep : IViewStep<DocumentEditorViewModel>
    {
        public async Task ExecuteAsync(IContext context, DocumentEditorViewModel vm)
        {
            await vm.CloseAsync();
        }
    }
}
