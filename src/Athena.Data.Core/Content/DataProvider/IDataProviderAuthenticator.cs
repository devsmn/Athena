namespace Athena.Data.Core
{
    public interface IDataProviderAuthenticator
    {
        Task<bool> AuthenticateAsync(string cipher);
    }
}
