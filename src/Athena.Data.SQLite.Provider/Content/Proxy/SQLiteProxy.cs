using System.Diagnostics;
using Athena.Data.Core;
using Athena.DataModel;
using Athena.DataModel.Core;

namespace Athena.Data.SQLite.Proxy
{
    public class SqliteProxy : IDataProxy
    {
        private SqliteManager _manager;
        private SqliteManager Manager => _manager ??= new SqliteManager();

        public TRepository Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : class, IAthenaRepository
        {
            try
            {
                IAthenaRepository instance = null;
                SqLiteProxyParameter sqlParameter = (SqLiteProxyParameter)parameter;

                if (typeof(TRepository) == typeof(IDocumentRepository))
                    instance = new SqliteDocumentRepository(sqlParameter.Cipher);

                if (typeof(TRepository) == typeof(IFolderRepository))
                    instance = new SqliteFolderRepository(sqlParameter.Cipher);

                if (typeof(TRepository) == typeof(IChapterRepository))
                    instance = new SqliteChapterRepository(sqlParameter.Cipher);

                if (typeof(TRepository) == typeof(ITagRepository))
                    instance = new SqliteTagRepository(sqlParameter.Cipher);

                return instance as TRepository;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }

            return null;
        }

        public IDataProviderPatcher RequestPatcher()
        {
            return Manager;
        }

        public IDataProviderAuthenticator RequestAuthenticator()
        {
            return Manager;
        }

        public IDataIntegrityValidator RequestIntegrityValidator()
        {
            return Manager;
        }
    }
}
