namespace Athena.UI
{
    using Athena.DataModel;

    public class DataPublishedEventArgs : EventArgs
    {
        private IList<RequestUpdate<Folder>> _folders;
        private IList<RequestUpdate<Document>> _documents;
        private IList<RequestUpdate<Page>> _pages;
        private IList<RequestUpdate<Tag>> _tags;

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

        public IList<RequestUpdate<Tag>> Tags
        {
            get { return _tags; }
            private set { _tags = value; }
        }

        public DataPublishedEventArgs()
        {
            Folders = new List<RequestUpdate<Folder>>();
            Documents = new List<RequestUpdate<Document>>();
            Pages = new List<RequestUpdate<Page>>();
            Tags = new List<RequestUpdate<Tag>>();
        }

    }

}
