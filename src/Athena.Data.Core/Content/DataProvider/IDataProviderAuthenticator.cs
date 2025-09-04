namespace Athena.Data.Core
{
    public interface IDataProviderAuthenticator
    {
        /// <summary>
        /// Authenticates the data provider with the given <paramref name="cipher"/>.
        /// </summary>
        /// <param name="cipher"></param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(string cipher);

        /// <summary>
        /// Authenticates the data provider at the given location with the provided <paramref name="cipher"/>.
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="dbPath"></param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(string cipher, string dbPath);
    }
}
