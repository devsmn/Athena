using Athena.Data.Core;
using Athena.DataModel;
using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.SQLite.Proxy
{
    public class SQLiteProxy : IDataProxy
    {
        public static IAthenaRepository Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : IAthenaRepository
        {
            try
            {
                if (typeof(TRepository) == typeof(IDocumentRepository))
                    return new SQLiteDocumentRepository();

                if (typeof(TRepository) == typeof(IFolderRepository))
                    return new SQLiteFolderRepository();

                if (typeof(TRepository) == typeof(IPageRepository))
                    return new SQLitePageRepository();

                if (typeof(TRepository) == typeof(IChapterRepository))
                    return new SQLiteChapterRepository();

                if (typeof(TRepository) == typeof(ITagRepository))
                    return new SQLiteTagRepository();

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
