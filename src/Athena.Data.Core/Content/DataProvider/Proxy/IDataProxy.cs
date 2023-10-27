using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.Core
{
    /// <summary>
    /// <see cref="IDataProxy"/> provides common functionality to retrieve data stores.
    /// </summary>
    public interface IDataProxy
    {
        static Type Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : IAthenaRepository => null;
    }
}
