using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Media;
using Android.Views.TextClassifiers;

namespace Athena.UI
{
    using Athena.DataModel;
    using Athena.DataModel.Core;
    
    public class DataPublishedEventArgs : EventArgs
    {
        private IList<RequestUpdate<Folder>> _folders;
        private IList<RequestUpdate<Document>> _documents;
        private IList<RequestUpdate<Page>> _pages;

        public IList<RequestUpdate<Folder>> Folders
        {
            get { return _folders; }
            private set { _folders = value; }
        }

        public IList<RequestUpdate<Document>> Documents
        {
            get { return _documents; }
            private set { _documents = value; }
        }

        public IList<RequestUpdate<Page>> Pages
        {
            get { return _pages; }
            private set { _pages = value; }
        }
        
        public DataPublishedEventArgs()
        {
            Folders = new List<RequestUpdate<Folder>>();
            Documents = new List<RequestUpdate<Document>>();
            Pages = new List<RequestUpdate<Page>>();
        }

    }

}
