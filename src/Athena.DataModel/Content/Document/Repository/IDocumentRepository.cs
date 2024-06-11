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

        /// <summary>
        /// Deletes the given <paramref name="document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        void Delete(IContext context, Document document);

        /// <summary>
        /// Reads the <see cref="Document"/> with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Document Read(IContext context, DocumentKey key);

        /// <summary>
        /// Adds the <paramref name="tag"/> to the given <paramref name="document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        /// <param name="tag"></param>
        void AddTag(IContext context, Document document, Tag tag);

        /// <summary>
        /// Deletes the <paramref name="tag"/> from the given <paramref name="document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        /// <param name="tag"></param>
        void DeleteTag(IContext context, Document document, Tag tag);

        /// <summary>
        /// Reads all tags of the given <paramref name="document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        IEnumerable<Tag> ReadTags(IContext context, Document document);

        /// <summary>
        /// Searches documents with the given <paramref name="documentName"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="documentName"></param>
        /// <param name="tags"></param>
        /// <param name="useFts"></param>
        /// <returns></returns>
        IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFts);

        /// <summary>
        /// Reads the PDF as a string.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        string ReadPdfAsString(IContext context, Document document);
    }
}
