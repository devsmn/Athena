using System.Diagnostics;
using Athena.Data.Core;
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    internal class SqliteManager : IDataProviderPatcher, IDataProviderAuthenticator
    {
        public void RegisterPatches(ICompatibilityService compatService)
        {
            VersionPatch encryptDatabase = new(162, ExecuteEncryptDatabasePatch);
            compatService.RegisterPatch<SqliteManager>(encryptDatabase);
        }

        public async Task ExecutePatchesAsync(IContext context, ICompatibilityService service)
        {
            foreach (VersionPatch patch in service.GetPatches<SqliteManager>())
            {
                await patch.PatchAsync(context);
            }
        }

        private async Task ExecuteEncryptDatabasePatch(IContext context)
        {
            SQLiteAsyncConnection encryptedDb = null;

            try
            {
                bool copyData = File.Exists(Defines.UnsafeDatabasePath);

                ISecureStorageService secureService = Services.GetService<ISecureStorageService>();
                IDataEncryptionService encryptionService = Services.GetService<IDataEncryptionService>();

                // Prepare the android key store to store the sql cipher key.
                context?.Log("Preparing database encryption");
                encryptionService.Initialize(context, IDataEncryptionService.DatabaseAlias);

                // Get the sql cipher key.
                context?.Log("Generating encryption key");
                string sqlCipherKey = secureService.GenerateRandomKey();

                // Encrypt the key and store it.
                context?.Log("Storing encryption key");

                // We need a fallback password in case biometrics do not work anymore or are not available at all.
                string fallbackPassword = string.Empty;

                IPasswordService passwordService = Services.GetService<IPasswordService>();
                await passwordService.New(context, newPassword => { fallbackPassword = newPassword; });

                // Save the key secured via the fallback password and the biometrics, if available.
                await encryptionService.SaveAsync(context, IDataEncryptionService.DatabaseAlias, sqlCipherKey, fallbackPassword);

                // Create the encrypted database.
                context?.Log("Connecting to encrypted database");
                SQLiteConnectionString encryptedOptions = new SQLiteConnectionString(Defines.DatabasePath, true, key: sqlCipherKey);
                encryptedDb = new SQLiteAsyncConnection(encryptedOptions);

                // Insert a dummy table to trigger the encryption.
                string dummyInsert = await SqliteRepository.ReadResourceAsync("CREATE_TABLE_META.sql");
                await encryptedDb.ExecuteAsync(dummyInsert);

                if (copyData)
                {
                    // Copy the old database to the new one.
                    context?.Log("Copying old data to new safe database");
                    await encryptedDb.ExecuteAsync("ATTACH DATABASE '$?' AS plaintext KEY '';", Defines.UnsafeDatabasePath);
                    await encryptedDb.ExecuteAsync("SELECT sqlcipher_export('main');");
                    await encryptedDb.ExecuteAsync("DETACH DATABASE plaintext;");

                    // Finally, delete the old database.
                    context?.Log("Removing old database");
                    File.Delete(Defines.UnsafeDatabasePath);
                }

                context?.Log("Successfully created secured data storage");
            }
            catch (Exception ex)
            {
                context?.Log("An error occurred while patching the global data storage");
                Debug.WriteLine(ex);
            }
            finally
            {
                if (encryptedDb != null)
                {
                    await encryptedDb.CloseAsync();
                }
            }
        }

        public async Task<bool> AuthenticateAsync(string cipher)
        {
            try
            {
                SqliteRepository repo = new SqliteRepository(cipher);
                await repo.ValidateConnection();

                return repo.IsValid;
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }
    }
}
