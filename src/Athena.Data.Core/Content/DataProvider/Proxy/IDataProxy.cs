using Athena.DataModel.Core;

namespace Athena.Data.Core
{
    /// <summary>
    /// <see cref="IDataProxy"/> provides common functionality to retrieve data stores.
    /// </summary>
    public interface IDataProxy
    {
        /// <summary>
        /// Requests an instance of a <see cref="IAthenaRepository"/> of the given <typeparamref name="TRepository"/>.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        TRepository Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : class, IAthenaRepository;

        /// <summary>
        /// Requests the <see cref="IDataProviderPatcher"/> for this <see cref="IDataProxy"/>.
        /// </summary>
        /// <returns></returns>
        IDataProviderPatcher RequestPatcher();

        /// <summary>
        /// Requests the <see cref="IDataProviderAuthenticator"/> for this <see cref="IDataProxy"/>.
        /// </summary>
        /// <returns></returns>
        IDataProviderAuthenticator RequestAuthenticator();
    }
}
