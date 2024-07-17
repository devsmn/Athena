using Athena.DataModel.Core;

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
