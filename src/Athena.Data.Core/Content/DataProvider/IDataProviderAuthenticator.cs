namespace Athena.Data.Core
{
    public interface IDataProviderAuthenticator
    {
        /// <summary>
        /// Authenticates a data provider with the given <paramref name="cipher"/>.
        /// </summary>
        /// <param name="cipher"></param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(string cipher);
    }
}
