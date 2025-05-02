using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;
using Athena.UI;

namespace Athena.UI
{

    internal class DocumentCloseStep : IViewStep<DocumentEditorViewModel>
    {
        public async Task ExecuteAsync(IContext context, DocumentEditorViewModel vm)
        {
            await vm.CloseAsync();
        }
    }
}
