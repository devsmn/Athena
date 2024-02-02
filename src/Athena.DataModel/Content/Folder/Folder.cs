using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder : Entity<FolderKey>
    {
        private string name;
        private string comment;
        private bool isPinned;
        private int isPinnedInt;

        private bool _pagesLoaded;

        private IList<Page> pages;

        public new DateTime CreationDate
        {
            get { return base.CreationDate; }
            set { base.CreationDate = value; }
        }

        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
        }

        public Folder(FolderKey key)
            : base(key)
        {
            pages = new List<Page>();
        }

        public Folder(int key)
            : this(new FolderKey(key))
        {
        }

        public Folder()
            : this(FolderKey.TemporaryId)
        {
        }

        public int IsPinnedInt
        {
            get { return isPinnedInt; }
            set
            {
                isPinnedInt = value;
                IsPinned = Convert.ToBoolean(value);
            }
        }

        public bool IsPinned
        {
            get { return isPinned; }
            set
            {
                isPinned = value;
                isPinnedInt = Convert.ToInt32(value);
            }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Comment
        {
            get { return comment; }
            set { this.comment = value; }
        }

        public IList<Page> Pages
        {
            get
            {
                if (!_pagesLoaded)
                {
                    pages = new List<Page>(ReadAllPages(new AthenaContext()));
                    _pagesLoaded = true;
                }

                return pages;
            }
            set { pages = value; }
        }
        
        public override string ToString()
        {
            return $"Id=[{Key?.Id}];Name=[{Name}]";
        }
    }
}
