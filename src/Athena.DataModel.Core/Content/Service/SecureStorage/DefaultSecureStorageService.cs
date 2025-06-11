using System.Diagnostics;
using System.Security.Cryptography;

namespace Athena.DataModel.Core
{
    public class DefaultSecureStorageService : ISecureStorageService
    {
        public async Task SaveAsync(string alias, string key)
        {
            await SecureStorage.SetAsync(alias, key);
        }

        public async Task<string> GetAsync(string alias)
        {
            try
            {
                Debug.WriteLine("Getting key: " + alias);
                return await SecureStorage.GetAsync(alias);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return null;
        }

        public string GenerateRandomKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public Task<string> GetDatabaseEncryptionKey(string pin)
        {
            throw new NotImplementedException();
        }
    }
}
