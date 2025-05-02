using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Document
    {
        /// <summary>
        /// Saves this <see cref="Document"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Save(IContext context)
        {
            DataStore.Resolve<IDocumentRepository>().Save(context, this);
        }

        /// <summary>
        /// Reads all tags.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Tag> ReadAllTags(IContext context)
        {
            return DataStore.Resolve<IDocumentRepository>().ReadTags(context, this);
        }

        /// <summary>
        /// Reads the <see cref="Document"/> with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Document Read(IContext context, IntegerEntityKey id)
        {
            return DataStore.Resolve<IDocumentRepository>().Read(context, id);
        }

        /// <summary>
        /// Deletes this <see cref="Document"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Delete(IContext context)
        {
            DataStore.Resolve<IDocumentRepository>().Delete(context, this);
        }

        /// <summary>
        /// Adds the given <paramref name="tag"/> to this <see cref="Document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        public void AddTag(IContext context, Tag tag)
        {
            DataStore.Resolve<IDocumentRepository>().AddTag(context, this, tag);
        }

        /// <summary>
        /// Deletes the given <paramref name="tag"/> from this <see cref="Document"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        public void DeleteTag(IContext context, Tag tag)
        {
            DataStore.Resolve<IDocumentRepository>().DeleteTag(context, this, tag);
        }

        /// <summary>
        /// Searches for documents with the given <paramref name="documentName"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="documentName"></param>
        /// <param name="tags"></param>
        /// <param name="searchFullText"></param>
        /// <returns></returns>
        public static IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool searchFullText)
        {
            return DataStore.Resolve<IDocumentRepository>().Search(context, documentName, tags, searchFullText);
        }

        /// <summary>
        /// Reads the PDF.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="document"></param>
        public void ReadPdf(IContext context, Document document)
        {
            Pdf = DataStore.Resolve<IDocumentRepository>().ReadPdf(context, document);
        }

        /// <summary>
        /// Moves this <see cref="Document"/> to the given <see cref="newFolder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="oldFolder"></param>
        /// <param name="newFolder"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void MoveTo(IContext context, Folder oldFolder, Folder newFolder)
        {
            DataStore.Resolve<IDocumentRepository>().MoveTo(context, this, oldFolder, newFolder);
        }

        /// <summary>
        /// Reads recent documents.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static IEnumerable<Document> ReadRecent(IContext context, int limit)
        {
            return DataStore.Resolve<IDocumentRepository>().ReadRecent(context, limit);
        }

        /// <summary>
        /// Counts all documents.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int CountAll(IContext context)
        {
            return DataStore.Resolve<IDocumentRepository>().CountAll(context);
        }
    }
}
