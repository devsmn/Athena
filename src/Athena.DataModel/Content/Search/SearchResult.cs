namespace Athena.DataModel
{
    public class SearchResult
    {
        public string DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPageNumber { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public string PageId { get; set; }
        public string PageTitle { get; set; }
        public string Snippet { get; set; }

        public Document Document { get; set; }
        public Page Page { get; set; }
        public Folder Folder { get; set; }

        public SearchResult()
        {
        }

        public SearchResult(int documentId, int documentPageNr, string snippet)
        {
            this.DocumentId = documentId.ToString();
            this.DocumentPageNumber = documentPageNr.ToString();
            this.Snippet = snippet;
        }

        public void Fill()
        {
            Document = new Document(Convert.ToInt32(DocumentId))
            {
                Name = DocumentName,
            };

            Folder = new Folder(Convert.ToInt32(FolderId)) {
                Name = FolderName
            };
        }
    }
}
