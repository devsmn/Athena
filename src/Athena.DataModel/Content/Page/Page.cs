using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Page : Entity<PageKey>
    {
        private bool _documentsLoaded;
        private string _title;
        private string _comment;

        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
        }

        private IList<Document> _documents;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public IList<Document> Documents
        {
            get
            {
                if (!_documentsLoaded)
                {
                    _documents = new List<Document>(ReadAllDocuments(new AthenaContext()));
                    _documentsLoaded = true;
                }
                return _documents;

            }
            set { _documents = value; }
        }

        public Page(PageKey value) : base(value)
        {
            _documents = new List<Document>();
        }
        

        public Page(int key)
            : this(new PageKey(key))
        {
        }

        public Page()
            : this(PageKey.TemporaryId)
        {
        }
    }
}
