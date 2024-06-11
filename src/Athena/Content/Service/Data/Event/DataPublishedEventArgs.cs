namespace Athena.UI
{
    using Athena.DataModel;

    public class DataPublishedEventArgs : EventArgs
    {
        public IList<RequestUpdate<Folder>> Folders { get; private set; }
        public IList<RequestUpdate<Document>> Documents { get; private set; }
        public IList<RequestUpdate<Page>> Pages { get; private set; }
        public IList<RequestUpdate<Tag>> Tags { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DataPublishedEventArgs"/>.
        /// </summary>
        public DataPublishedEventArgs()
        {
            Folders = new List<RequestUpdate<Folder>>();
            Documents = new List<RequestUpdate<Document>>();
            Pages = new List<RequestUpdate<Page>>();
            Tags = new List<RequestUpdate<Tag>>();
        }
    }
}
