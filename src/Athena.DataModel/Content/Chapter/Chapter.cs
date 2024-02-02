using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Chapter
    {
        public string DocumentId { get; set; }
        public string DocumentPageNumber { get; set; }
        public string FolderId { get; set; }
        public string PageId { get; set; }
        public string Snippet { get; set; }
        
        public Document Document { get; set; }
        public Page Page { get; set; }
        public Folder Folder { get; set; }

        public Chapter()
        {
        }

        public Chapter(int documentId, int documentPageNr, string snippet)
        {
            this.DocumentId = documentId.ToString();
            this.DocumentPageNumber = documentPageNr.ToString();
            this.Snippet = snippet;
        }
    }
}
