using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.LifecycleEvents;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class DocumentDetailsViewModel : ContextViewModel
    {
        private readonly Page _page;

        [ObservableProperty]
        private DocumentViewModel _document;
        
        public DocumentDetailsViewModel(Page page, Document document)
        {
            _page = page;
            Document = document;
        }
    }
}
