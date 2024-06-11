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
            DocumentId = documentId.ToString();
            DocumentPageNumber = documentPageNr.ToString();
            Snippet = snippet;
        }
    }
}
