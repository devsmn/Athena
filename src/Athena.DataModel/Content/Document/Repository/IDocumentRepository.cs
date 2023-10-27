using Athena.DataModel.Core;
using Microsoft.VisualBasic.FileIO;

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
    }
}
