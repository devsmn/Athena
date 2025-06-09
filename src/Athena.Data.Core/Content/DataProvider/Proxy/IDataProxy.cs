using Athena.DataModel.Core;

namespace Athena.Data.Core
{
    /// <summary>
    /// <see cref="IDataProxy"/> provides common functionality to retrieve data stores.
    /// </summary>
    public interface IDataProxy
    {
        TRepository Request<TRepository>(IDataProxyParameter parameter) where TRepository : class, IAthenaRepository;
        IDataProviderPatcher RequestPatcher();
    }
}
