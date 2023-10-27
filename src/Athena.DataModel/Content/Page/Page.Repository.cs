using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var documents = DataStore.Resolve<IDocumentRepository>().ReadAll(context, this);

            foreach (var document in documents)
            {
                this.AddDocument(document);
            }

            return this.Documents;
        }

        public void Delete(IContext context)
        {
            DataStore.Resolve<IPageRepository>().Delete(context, this);
        }
        
        
    }
}
