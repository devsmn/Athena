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

                if (typeof(TRepository) == typeof(IPageRepository))
                    return new SqLitePageRepository();

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
