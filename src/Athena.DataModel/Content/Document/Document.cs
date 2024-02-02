using Athena.DataModel.Core;
using System.IO.Compression;

namespace Athena.DataModel
{
    public partial class Document : Entity<DocumentKey>
    {
        private string _name;
        private string _comment;
        private byte[] _pdf;
        private string _pdfString;
        private byte[] _thumbnail;
        private string _thumbnailString;

        private bool _tagsLoaded;
        private bool _pdfRead;

        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
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
                    ReadPdf(new AthenaContext(), this);
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
                    _tags = new List<Tag>(this.ReadAllTags(new AthenaContext()));
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

        public Document(DocumentKey key) : base(key)
        {
            _tags = new();
        }

        public Document()
            : this(new DocumentKey(DocumentKey.TemporaryId))
        {
        }
    }
}
