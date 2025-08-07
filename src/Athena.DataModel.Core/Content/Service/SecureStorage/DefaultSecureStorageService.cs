using System.Diagnostics;
using System.Security.Cryptography;

namespace Athena.DataModel.Core
{
    public class DefaultSecureStorageService : ISecureStorageService
    {
        public async Task SaveAsync(string alias, string key)
        {
            Debug.WriteLine($"Storing alias {alias} = [{key}]");
            await SecureStorage.SetAsync(alias, key);
        }

        public async Task<string> GetAsync(string alias)
        {
            string val = await SecureStorage.GetAsync(alias);
            Debug.WriteLine($"Retrieved alias {alias} = [{val}]");
            return val;
        }

        public string GenerateRandomKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}
