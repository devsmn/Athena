using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public partial class Document
    {
        public void Save(IContext context)
        {
            DataStore.Resolve<IDocumentRepository>().Save(context, this);
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
    }
}
