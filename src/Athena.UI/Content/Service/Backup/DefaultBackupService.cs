using System.IO.Compression;
using System.Text.Json;
using Athena.Data.Core;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Athena.DataModel.Core;
using CommunityToolkit.Maui.Storage;

namespace Athena.UI
{
    public enum BackupRestoreResultCode
    {
        GenericError,
        InvalidFileExtension,
        MissingMetaData,
        InvalidVersion,
        AbortedByUser,
        IntegrityNotValid,
        Success
    }

    public class BackupRestoreResult
    {
        public BackupRestoreResultCode Code { get; set; }
        public bool IsSuccess => Code == BackupRestoreResultCode.Success;
        public string ErrorInfo { get; set; }
    }

    public class DefaultBackupService : IBackupService
    {
        private readonly ICompressionService _compressionService;

        public DefaultBackupService()
        {
            _compressionService = Services.GetService<ICompressionService>();
        }

        public async Task<FileSaverResult> Create(IContext context)
        {
            try
            {
                context.Log("Creating backup meta data");
                BackupMetaData metaData = new();
                metaData.CreationDate = DateTime.UtcNow.Ticks;
                metaData.DocumentCount = Document.CountAll(context);
                metaData.FolderCount = Folder.CountAll(context);
                metaData.TagCount = 0;
                metaData.Initiator = "User";
                metaData.AppVersion = $"{AppInfo.Current.VersionString}-{AppInfo.Current.BuildString}";
                metaData.MinVersion = Convert.ToInt32(AppInfo.Current.BuildString);

                using (MemoryStream zipStream = new())
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        archive.CreateEntryFromFile(Defines.DatabasePath, "data.db3");

                        ZipArchiveEntry metaDataEntry = archive.CreateEntry("metadata.json");

                        using (Stream jsonStream = metaDataEntry.Open())
                        {
                            using (StreamWriter sw = new(jsonStream))
                            {
                                string json = JsonSerializer.Serialize(metaData);
                                await sw.WriteAsync(json);
                            }
                        }
                    }

                    zipStream.Seek(0, SeekOrigin.Begin);

                    context.Log("Creating backup");
                    return await FileSaver.SaveAsync($"backup_{DateTime.Now:yyyyMMdd_HHmmss}.athbck", zipStream);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return null;
        }

        public async Task<BackupRestoreResult> Restore(
            IContext context,
            string path,
            Func<string, string, string, Task<bool>> requireUserConfirmation,
            Action<bool> showProgressIndicator)
        {
            BackupRestoreResult result = new();
            try
            {
                if (Path.GetExtension(path) != ".athbck")
                {
                    result.Code = BackupRestoreResultCode.InvalidFileExtension;
                    return result;
                }

                context.Log("Extracting backup data");
                string tmpDir = Path.Combine(FileSystem.CacheDirectory, "restore", Guid.NewGuid().ToString());
                ZipFile.ExtractToDirectory(path, tmpDir, overwriteFiles: true);

                string jsonPath = Path.Combine(tmpDir, "metadata.json");
                string dbPath = Path.Combine(tmpDir, "data.db3");
                BackupMetaData metaData = JsonSerializer.Deserialize<BackupMetaData>(await File.ReadAllTextAsync(jsonPath));

                if (metaData == null)
                {
                    result.Code = BackupRestoreResultCode.MissingMetaData;
                    return result;
                }

                int minVersion = Convert.ToInt32(AppInfo.Current.BuildString);

                showProgressIndicator(false);

                if (minVersion < metaData.MinVersion)
                {
                    result.ErrorInfo = $"Backup version: {metaData.MinVersion}, required version: {minVersion}";
                    result.Code = BackupRestoreResultCode.InvalidVersion;
                    return result;
                }

                string importInfo =
                    $"The following backup will be restored: {Environment.NewLine}{Environment.NewLine}" +
                    $"Documents: {metaData.DocumentCount}{Environment.NewLine}" +
                    $"Folders: {metaData.FolderCount}{Environment.NewLine}" +
                    $"Tags: {metaData.TagCount}{Environment.NewLine}{Environment.NewLine}" +
                    $"The backup was created at {new DateTime(metaData.CreationDate, DateTimeKind.Utc).ToLocalTime()} " +
                    $"with app version {metaData.AppVersion}";

                bool canContinue = await MainThread.InvokeOnMainThreadAsync(async () => await requireUserConfirmation(importInfo, "Ok", "Cancel"));

                if (!canContinue)
                {
                    result.Code = BackupRestoreResultCode.AbortedByUser;
                    return result;
                }

                bool externalKeyRequired =  await MainThread.InvokeOnMainThreadAsync(async () => await requireUserConfirmation(
                    "Did you create the backup on a different device, have you reinstalled Athena or did you change the password after creating the backup?",
                    "Yes",
                    "No"));

                context.Log("Decrypting restored database");
                string cipher = string.Empty;
                SqliteProxy sqlProxy = new();
                IDataProviderAuthenticator sqlAuth = sqlProxy.RequestAuthenticator();
                IDataIntegrityValidator dataIntegrityValidator = sqlProxy.RequestIntegrityValidator();
                IPasswordService passwordService = Services.GetService<IPasswordService>();
                IDataEncryptionService encryptionService = Services.GetService<IDataEncryptionService>();

                if (externalKeyRequired)
                {
                    bool firstTry = true;
                    bool exit = false;

                    do
                    {
                        await passwordService.Prompt(context, !firstTry, (str) => cipher = str, () => exit = true);
                        firstTry = false;

                        if (exit)
                        {
                            result.Code = BackupRestoreResultCode.AbortedByUser;
                            return result;
                        }

                    } while (await sqlAuth.AuthenticateAsync(cipher, dbPath) == false);
                }
                else
                {
                    // Try to open the database with the stored encryption key
                    bool primarySucceeded = await encryptionService.ReadPrimaryAsync(context, IDataEncryptionService.DatabaseAlias, (c) => cipher = c, context.Log, () => {});

                    if (!primarySucceeded || !await sqlAuth.AuthenticateAsync(cipher, dbPath))
                    {
                        bool firstTry = true;

                        do
                        {
                            string pin = string.Empty;
                            bool cancelled = false;

                            await passwordService.Prompt(
                                context,
                                !firstTry,
                                (str) => pin = str,
                                () => cancelled = true);

                            if (cancelled)
                            {
                                result.Code = BackupRestoreResultCode.AbortedByUser;
                                return result;
                            }

                            await encryptionService.ReadFallbackAsync(
                                context,
                                IDataEncryptionService.DatabaseAlias,
                                pin, key => cipher = key,
                                context.Log);

                            firstTry = false;

                        } while (await sqlAuth.AuthenticateAsync(cipher, dbPath) == false);
                    }
                }

                showProgressIndicator(true);
                await Task.Delay(200);
                context.Log("Validating database integrity");

                // Valid cipher was supplied. Verify the integrity of the database.
                bool isValid = await dataIntegrityValidator.ValidateAsync(context, cipher, dbPath);

                if (!isValid)
                {
                    result.Code = BackupRestoreResultCode.IntegrityNotValid;
                    return result;
                }

                context.Log("Replacing current database with restored one");

                // All checks done, replace the databases.
                string oldDbBackupPath = Path.Combine(tmpDir, "prev.db3");
                File.Move(Defines.DatabasePath, oldDbBackupPath, true);
                File.Move(dbPath, Defines.DatabasePath, true);

                context.Log("Validating database integrity after replace");

                // Verify the integrity again, just to be sure nothing broke while copying.
                isValid = await dataIntegrityValidator.ValidateAsync(context, cipher, dbPath);

                if (!isValid)
                {
                    context.Log("Restoring old database");

                    // Not good. Restore the initial db.
                    File.Move(oldDbBackupPath, Defines.DatabaseFileName, true);
                    result.Code = BackupRestoreResultCode.IntegrityNotValid;
                    return result;
                }

                if (externalKeyRequired)
                {
                    context.Log("Storing external encryption");
                    // The user provided an external, correct cipher. Store it.
                    await encryptionService.DeleteAsync(context, IDataEncryptionService.DatabaseAlias);

                    string pw = string.Empty;
                    await passwordService.New(context, userPw => pw = userPw);

                    encryptionService.Initialize(context, IDataEncryptionService.DatabaseAlias);
                    await encryptionService.SaveAsync(context, IDataEncryptionService.DatabaseAlias, cipher, pw);
                }

                Directory.Delete(tmpDir, true);
                IPreferencesService prefService = Services.GetService<IPreferencesService>();

                // Set the version to trigger any patches when restarting the app.
                prefService.SetLastTermsOfUseVersion(metaData.MinVersion); 

                result.Code = BackupRestoreResultCode.Success;
            }
            catch (Exception ex)
            {
                result.Code = BackupRestoreResultCode.GenericError;
                result.ErrorInfo = ex.Message;
                context.Log(ex);
            }

            return result;
        }
    }
}
