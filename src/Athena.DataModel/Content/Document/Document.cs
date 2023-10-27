using Athena.DataModel.Core;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public partial class Document : Entity<DocumentKey>
    {
        private byte[] image;
        private string name;
        private string comment;
        private string imageString; 
        
        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public byte[] Image
        {
            get { return image; }
            set
            {
                image = value;
            }
        }

        public string ImageString
        {
            get { return imageString; }
            set
            {
                imageString = value;
                image = Convert.FromBase64String(value);
            }
        }

        public Document(DocumentKey key) : base(key)
        {
        }

        public Document()
            : this(new DocumentKey(DocumentKey.TemporaryId))
        {
        }
    }
}
