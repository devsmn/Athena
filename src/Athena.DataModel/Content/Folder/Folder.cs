using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder : Entity<FolderKey>
    {
        private string _name;
        private string _comment;
        private bool _isPinned;
        private int _isPinnedInt;
        private bool _pagesLoaded;

        private IList<Page> _pages;

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
            _pages = new List<Page>();
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
            get { return _isPinnedInt; }
            set
            {
                _isPinnedInt = value;
                IsPinned = Convert.ToBoolean(value);
            }
        }

        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                _isPinned = value;
                _isPinnedInt = Convert.ToInt32(value);
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public IList<Page> Pages
        {
            get
            {
                if (!_pagesLoaded)
                {
                    _pages = new List<Page>(ReadAllPages(new AthenaContext()));
                    _pagesLoaded = true;
                }

                return _pages;
            }
            set { _pages = value; }
        }
        
        public override string ToString()
        {
            return $"Id=[{Key?.Id}];Name=[{Name}]";
        }
    }
}
