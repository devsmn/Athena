using Athena.Data.Core;
using Athena.DataModel;
using Athena.DataModel.Core;
using System.Diagnostics;

namespace Athena.Data.SQLite.Proxy
{
    public class SqLiteProxy : IDataProxy
    {
        public static IAthenaRepository Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : IAthenaRepository
        {
            try
            {
                if (typeof(TRepository) == typeof(IDocumentRepository))
                    return new SqLiteDocumentRepository();

                if (typeof(TRepository) == typeof(IFolderRepository))
                    return new SqLiteFolderRepository();

                if (typeof(TRepository) == typeof(IChapterRepository))
                    return new SqLiteChapterRepository();

                if (typeof(TRepository) == typeof(ITagRepository))
                    return new SqLiteTagRepository();

                throw new ArgumentOutOfRangeException();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }

            return null;
        }
    }
}
