using System.Diagnostics;
using Athena.Data.Core;
using Athena.DataModel;
using Athena.DataModel.Core;

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
                    return new SqliteDocumentRepository();

                if (typeof(TRepository) == typeof(IFolderRepository))
                    return new SqliteFolderRepository();

                if (typeof(TRepository) == typeof(IChapterRepository))
                    return new SqliteChapterRepository();

                if (typeof(TRepository) == typeof(ITagRepository))
                    return new SqliteTagRepository();

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
