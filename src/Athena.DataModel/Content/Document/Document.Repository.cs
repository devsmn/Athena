using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Document
    {
        public void Save(IContext context)
        {
            DataStore.Resolve<IDocumentRepository>().Save(context, this);
        }

        public IEnumerable<Tag> ReadAllTags(IContext context)
        {
            return DataStore.Resolve<IDocumentRepository>().ReadTags(context, this);
        }

        public static Document Read(IContext context, DocumentKey id)
        {
            return DataStore.Resolve<IDocumentRepository>().Read(context, id);
        }

        public IEnumerable<Document> LoadAll(IContext context)
        {
            throw new NotImplementedException();
        }

        public Document Load(IContext context)
        {
            throw new NotImplementedException();
        }

        public void Delete(IContext context)
        {
            DataStore.Resolve<IDocumentRepository>().Delete(context, this);
        }

        public void AddTag(IContext context, Tag tag)
        {
            DataStore.Resolve<IDocumentRepository>().AddTag(context, this, tag);
        }

        public void DeleteTag(IContext context, Tag tag)
        {
            DataStore.Resolve<IDocumentRepository>().DeleteTag(context, this, tag);
        }

        public static IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool searchFullText)
        {
            return DataStore.Resolve<IDocumentRepository>().Search(context, documentName, tags, searchFullText);
        }

        public void ReadPdf(IContext context, Document document)
        {
            this.PdfString = DataStore.Resolve<IDocumentRepository>().ReadPdfAsString(context, document);
        }
    }
}
