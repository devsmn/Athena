using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Page
    {
        public void AddDocument(Document document)
        {
            this.Documents.Add(document);
        }

        public void Save(IContext context)
        {
            DataStore.Resolve<IPageRepository>().Save(context, this);
        }

        public void AddTag(Tag tag)
        {
            DataStore.Resolve<IPageRepository>().AddTag(tag);
        }

        public static IEnumerable<Page> ReadAll(IContext context)
        {
            return DataStore.Resolve<IPageRepository>().ReadAll(context);
        }

        public IEnumerable<Document> ReadAllDocuments(IContext context)
        {
            return DataStore.Resolve<IDocumentRepository>().ReadAll(context, this);
        }

        public void Delete(IContext context)
        {
            DataStore.Resolve<IPageRepository>().Delete(context, this);
        }


        public static Page Read(IContext context, PageKey pageKey)
        {
            return DataStore.Resolve<IPageRepository>().Read(context, pageKey);
        }
    }
}
