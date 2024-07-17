using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Document : Entity
    {
        private string _name;
        private string _comment;
        private byte[] _pdf;
        private string _pdfString;
        private byte[] _thumbnail;
        private string _thumbnailString;
        private bool _isPinned;

        private bool _tagsLoaded;
        private bool _pdfRead;

        public bool IsPinned
        {
            get { return _isPinned; }
            set { _isPinned = value; }
        }

        public int IsPinnedInteger
        {
            get { return Convert.ToInt32(IsPinned); }
            set { IsPinned = Convert.ToBoolean(value); }
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

        public byte[] Pdf
        {
            get
            {
                if (!_pdfRead)
                {
                    ReadPdf(new AthenaDataContext(), this);
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

        public string PdfString
        {
            get { return _pdfString; }
            set
            {
                _pdfString = value;
                _pdf = Convert.FromBase64String(value);
            }
        }

        public byte[] Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; }
        }

        public string ThumbnailString
        {
            get { return _thumbnailString; }
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
                    _tags = new List<Tag>(this.ReadAllTags(new AthenaDataContext()));
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
