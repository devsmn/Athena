using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public partial class Folder : Entity<FolderKey>
    {
        private string name;
        private string comment;
        private byte[] thumbnail;
        private string thumbnailString;

        private IList<Page> pages;

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

        public string ThumbnailString
        {
            get { return thumbnailString; }
            set
            {
                thumbnailString = value;

                if (string.IsNullOrEmpty(value))
                    thumbnail = null;
                else
                {
                    // Takes long to load?
                    thumbnail = Convert.FromBase64String(value);
                }
            }
        }

        public byte[] Thumbnail
        {
            get { return thumbnail; }
            set { thumbnail = value; }
        }

        public IList<Page> Pages
        {
            get { return pages; }
            set { pages = value; }
        }

        public int PageCount
        {
            get { return pages?.Count ?? 0; }
        }

        public override string ToString()
        {
            return $"Id=[{Key?.Id}];Name=[{Name}]";
        }
    }
}
