using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IDocumentRepository : IAthenaRepository
    {
        /// <summary>
        /// Saves the given <paramref name="document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        void Save(IContext context, Document document);

        /// <summary>
        /// Reads all available documents.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<Document> ReadAll(IContext context, Page page);

        void Delete(IContext context, Document document);

        Document Read(IContext context, DocumentKey key);

        void AddTag(IContext context, Document document, Tag tag);
        void DeleteTag(IContext context, Document document, Tag tag);
        IEnumerable<Tag> ReadTags(IContext context, Document document);

        IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFTS);

        string ReadPdfAsString(IContext context, Document document);
    }
}
