using Athena.Data.Core;
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    internal class SqliteManager : IDataProviderPatcher, IDataProviderAuthenticator, IDataIntegrityValidator
    {
        public void RegisterPatches(ICompatibilityService compatService)
        {
            VersionPatch encryptDatabase = new(163, ExecuteEncryptDatabasePatch);
            compatService.RegisterPatch<SqliteManager>(encryptDatabase);
        }

        public async Task ExecutePatchesAsync(IContext context, ICompatibilityService service)
        {
            foreach (VersionPatch patch in service.GetPatches<SqliteManager>(context))
            {
                await patch.PatchAsync(context);
            }
        }

        /// <summary>
        /// Executes the patch that converts the plain db into an encrypted database. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ExecuteEncryptDatabasePatch(IContext context)
        {
            SQLiteAsyncConnection encryptedDb = null;
            SQLiteConnection plainTextDb = null;

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

                context?.Log("Storing encryption key");

                // We need a fallback password in case biometrics do not work anymore or are not available at all.
                string fallbackPassword = string.Empty;

                IPasswordService passwordService = Services.GetService<IPasswordService>();
                await passwordService.New(context, newPassword => { fallbackPassword = newPassword; });

                // Save the key secured via the fallback password and the biometrics, if available.
                await encryptionService.SaveAsync(context, IDataEncryptionService.DatabaseAlias, sqlCipherKey, fallbackPassword);

                // Create the encrypted database.
                context?.Log("Connecting to encrypted database");

                if (copyData && System.Buffers.Text.Base64.IsValid(sqlCipherKey))
                {
                    // Copy the old database to the new one.
                    // Encrypted db will be created with sqlcipher_export.
                    context?.Log($"Copying old data from {Defines.UnsafeDatabasePath} to {Defines.DatabasePath}");

                    var connectionString = new SQLiteConnectionString(Defines.UnsafeDatabasePath, true, key: "");
                    plainTextDb = new SQLiteConnection(connectionString);

                    plainTextDb.Execute($"ATTACH DATABASE '{Defines.DatabasePath}' AS encrypted KEY '{sqlCipherKey}';");
                    plainTextDb.ExecuteScalar<string>("SELECT sqlcipher_export('encrypted');");
                    plainTextDb.Execute("DETACH DATABASE encrypted;");

                    plainTextDb.Close();
                    plainTextDb = null;

                    // Finally, delete the old database.
                    context?.Log("Removing old database");
                    File.Delete(Defines.UnsafeDatabasePath);
                }

                SQLiteConnectionString encryptedOptions = new SQLiteConnectionString(Defines.DatabasePath, true, key: sqlCipherKey);
                encryptedDb = new SQLiteAsyncConnection(encryptedOptions);

                // Insert a dummy table to trigger the encryption.
                string dummyInsert = await SqliteRepository.ReadResourceAsync("CREATE_TABLE_META.sql");
                await encryptedDb.ExecuteAsync(dummyInsert);

                context?.Log("Successfully created secured data storage");
            }
            catch (Exception ex)
            {
                context?.Log(ex);
            }
            finally
            {
                if (encryptedDb != null)
                {
                    await encryptedDb.CloseAsync();
                }

                plainTextDb?.Close();
            }
        }

        public async Task<bool> AuthenticateAsync(string cipher)
        {
            return await AuthenticateAsync(cipher, Defines.DatabasePath);
        }

        public async Task<bool> AuthenticateAsync(string cipher, string dbPath)
        {
            SqliteRepository repo = null;
            try
            {
                repo = new SqliteRepository(cipher, dbPath);
                await repo.ValidateConnection();

                return repo.IsValid;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (repo != null)
                    await repo.CloseAsync();
            }
        }

        public async Task<bool> ValidateAsync(IContext context, string cipher, string db)
        {
            SqliteRepository repo = null;
            try
            {
                repo = new SqliteRepository(cipher, db);
                return await repo.ValidateIntegrity(context);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
            finally
            {
                if (repo != null)
                    await repo.CloseAsync();
            }

            return false;
        }
    }
}
