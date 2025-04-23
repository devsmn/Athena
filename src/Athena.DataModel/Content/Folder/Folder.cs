using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder : Entity
    {
        private bool _isPinned;
        private int _isPinnedInt;

        private bool _foldersLoaded;
        private bool _documentsLoaded;

        private List<Document> _documents;
        private List<Folder> _folders;

        public Folder(IntegerEntityKey key)
            : base(key)
        {
            _folders = new List<Folder>();
            _documents = new List<Document>();
        }

        public Folder(int key)
            : this(new IntegerEntityKey(key))
        {
        }

        public Folder()
            : this(IntegerEntityKey.TemporaryId)
        {
        }

        public int IsPinnedInt
        {
            get => _isPinnedInt;
            set
            {
                _isPinnedInt = value;
                IsPinned = Convert.ToBoolean(value);
            }
        }

        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                _isPinned = value;
                _isPinnedInt = Convert.ToInt32(value);
            }
        }

        public string Name { get; set; }

        public string Comment { get; set; }

        public IEnumerable<Folder> LoadedFolders => _foldersLoaded ? Folders : Enumerable.Empty<Folder>();

        public IEnumerable<Document> LoadedDocuments => _documentsLoaded ? Documents : Enumerable.Empty<Document>();

        public List<Folder> Folders
        {
            get
            {
                if (!_foldersLoaded)
                {
                    _folders = new List<Folder>(ReadAllFolders(new AthenaDataContext()));
                    _foldersLoaded = true;
                }

                return _folders;
            }

            set => _folders = value;
        }


        public List<Document> Documents
        {
            get
            {
                if (!_documentsLoaded)
                {
                    _documents = new List<Document>(ReadAllDocuments(new AthenaDataContext()));
                    _documentsLoaded = true;
                }

                return _documents;
            }

            set => _documents = value;
        }

        public override string ToString()
        {
            return $"Id=[{Key?.Id}];Name=[{Name}]";
        }
    }
}
