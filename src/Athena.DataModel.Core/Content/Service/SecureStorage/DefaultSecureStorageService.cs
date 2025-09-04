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
            return await SecureStorage.GetAsync(alias);
        }

        public void Delete(string alias)
        {
            SecureStorage.Remove(alias);
        }

        public string GenerateRandomKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}
