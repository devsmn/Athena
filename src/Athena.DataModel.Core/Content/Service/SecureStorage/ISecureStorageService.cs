using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public interface ISecureStorageService
    {
        Task SaveAsync(string alias, string key);
        Task<string> GetAsync(string alias);
        Task<string> GetDatabaseEncryptionKey(string pin);
        string Generate256BitKey();
    }
}
