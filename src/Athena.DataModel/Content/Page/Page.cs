using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public partial class Page : Entity<PageKey>
    {
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
            get { return documents; }
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
