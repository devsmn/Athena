using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public interface IDataEncryptionService
    {
        /// <summary>
        /// Initializes the database cipher. This only has to be done for the first time.
        /// </summary>
        void InitializeDatabaseCipher(IContext context);

        /// <summary>
        /// Saves the given database cipher.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallbackPin"></param>
        /// <returns></returns>
        Task SaveDatabaseCipher(IContext context, string key, string fallbackPin);

        /// <summary>
        /// Reads the database cipher while authorizing against the primary source (biometrics).
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        Task<bool> ReadDatabaseCipherPrimary(IContext context, Action<string> onSuccess, Action<string> onError);

        /// <summary>
        /// Reads the database cipher while validating against the fallback source (PIN).
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        Task ReadDatabaseCipherFallback(IContext context, string pin ,Action<string> onSuccess, Action<string> onError);
    }
}
