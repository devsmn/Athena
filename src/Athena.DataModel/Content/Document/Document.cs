using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Document : Entity
    {
        private byte[] _pdf;
        private byte[] _thumbnail;
        private string _thumbnailString;

        private bool _tagsLoaded;
        private bool _pdfRead;

        public bool IsPinned { get; set; }

        public int IsPinnedInteger
        {
            get => Convert.ToInt32(IsPinned);
            set => IsPinned = Convert.ToBoolean(value);
        }

        public string Name { get; set; }

        public string Comment { get; set; }

        public byte[] Pdf
        {
            get
            {
                if (!_pdfRead)
                {
                    ReadPdf(new AthenaDataContext());
                    _pdfRead = true;
                }

                return _pdf;
            }
            set
            {
                _pdf = value;
                _pdfRead = true;
            }
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => _thumbnail = value;
        }

        public string ThumbnailString
        {
            get => _thumbnailString;
            set
            {
                _thumbnailString = value;
                _thumbnail = Convert.FromBase64String(value);
            }
        }

        private List<Tag> _tags;


        public List<Tag> Tags
        {
            get
            {
                if (!_tagsLoaded)
                {
                    _tags = new List<Tag>(ReadAllTags(new AthenaDataContext()));
                    _tagsLoaded = true;
                }
                return _tags;
            }
            set
            {
                _tags = value;
                _tagsLoaded = true;
            }
        }

        public Document(int id)
            : base(new IntegerEntityKey(id))
        {
            _tags = new();
        }
        public Document(IntegerEntityKey key) : base(key)
        {
            _tags = new();
        }

        public Document()
            : this(new IntegerEntityKey(IntegerEntityKey.TemporaryId))
        {
        }
    }
}
