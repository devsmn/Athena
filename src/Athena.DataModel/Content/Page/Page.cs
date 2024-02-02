using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Page : Entity<PageKey>
    {
        private bool _documentsLoaded;

        private string title;
        private string comment;
        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
        }

        private IList<Document> documents;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public IList<Document> Documents
        {
            get
            {
                if (!_documentsLoaded)
                {
                    documents = new List<Document>(ReadAllDocuments(new AthenaContext()));
                    _documentsLoaded = true;
                }
                return documents;

            }
            set { documents = value; }
        }

        public Page(PageKey value) : base(value)
        {
            documents = new List<Document>();
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
