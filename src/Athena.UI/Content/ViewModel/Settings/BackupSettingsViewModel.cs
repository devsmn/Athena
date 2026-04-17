using Athena.Data.Core;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    internal partial class BackupSettingsViewModel : ContextViewModel
    {
        public BackupSettingsViewModel()
        {
        }

        [RelayCommand]
        private async Task RestoreBackup()
        {
            IBackupService backupService = Services.GetService<IBackupService>();

            PickOptions options = new();
            options.PickerTitle = Localization.BackupSelectFile;
            var result = await FilePicker.PickAsync(options);

            if (result != null)
            {
                BackupRestoreResult restoreResult = null;

                await ExecuteAsyncBackgroundAction(async context =>
                {
                    await Task.Delay(200);

                    restoreResult = await backupService.RestoreAsync(
                        context,
                        result.FullPath,
                        RequireUserConfirmation,
                        (show) => MainThread.InvokeOnMainThreadAsync(() => IsBusy = show));
                });


                IsBusy = false;

                if (restoreResult.IsSuccess)
                {
                    Application.Current.Windows[0].Page = new ContainerPage();
                    await Toast.Make(Localization.BackupRestored, ToastDuration.Long).Show();
                }
                else
                {
                    if (restoreResult.Code == BackupRestoreResultCode.AbortedByUser)
                    {
                        await Toast.Make("Cancelled restoring backup", ToastDuration.Long).Show();
                        return;
                    }

                    IContext context = RetrieveContext();
                    context.Log(new Exception($"Error while restoring backup: {restoreResult.Code} - {restoreResult.ErrorInfo}"));

                    await Toast.Make(Localization.BackupRestoreFailed, ToastDuration.Long).Show();

                }
            }
            else
            {
                await Toast.Make(Localization.BackupSelectedFileInvalid, ToastDuration.Long).Show();
            }
        }

        private async Task<bool> RequireUserConfirmation(string msg, string okButton, string cancelButton)
        {
            return await DisplayAlert(Localization.BackupRestoreTitle, msg, okButton, cancelButton);
        }

        [RelayCommand]
        private async Task CreateBackup()
        {
            IContext context = RetrieveContext();

            bool @continue = await DisplayAlert(
                Localization.BackupEncryptionKey,
                Localization.BackupShowEncryptionKeyPreInfo,
                Localization.BackupShowKeyOption,
                Localization.Cancel);

            if (!@continue)
            {
                context.Log("User aborted backup creation");
                return;
            }

            string cipher = string.Empty;
            SqliteProxy sqlProxy = new();
            IDataProviderAuthenticator sqlAuth = sqlProxy.RequestAuthenticator();
            IDataEncryptionService encryptionService = Services.GetService<IDataEncryptionService>();
            string alias = await encryptionService.GetActiveAliasAsync();

            bool primarySucceeded = await encryptionService.ReadPrimaryAsync(context, alias, (c) => cipher = c, context.Log, () => { });

            if (!primarySucceeded || !await sqlAuth.AuthenticateAsync(cipher))
            {
                IPasswordService passwordService = Services.GetService<IPasswordService>();

                bool firstTry = true;

                do
                {
                    context.Log("Requesting fallback access to database");
                    string pin = string.Empty;
                    await passwordService.Prompt(context, Localization.PasswordUnlockData, !firstTry, (str) => pin = str);

                    await encryptionService.ReadFallbackAsync(
                        context,
                        alias,
                        pin, key => cipher = key,
                        error => context.Log(error));

                    firstTry = false;

                } while (!await sqlAuth.AuthenticateAsync(cipher));
            }

            bool copy = await DisplayAlert(
                Localization.BackupEncryptionKey,
                string.Format(Localization.BackupShowEncryptionKey, cipher),
                Localization.BackupCopyKey,
                Localization.Close);

            if (copy)
            {
                await Clipboard.SetTextAsync(cipher);
                await Toast.Make(Localization.BackupEncryptionKeyCopied, ToastDuration.Long).Show();
            }

            cipher = null;

            IBackupService backupService = Services.GetService<IBackupService>();
            FileSaverResult? result = await backupService.CreateAsync(context);

            if (result?.IsSuccessful == true)
            {
                await Toast.Make(Localization.BackupSavedSuccessfully, ToastDuration.Long).Show();
            }
            else
            {
                if (result?.Exception != null)
                {
                    context.Log(result.Exception);
                }

                await Toast.Make(Localization.BackupSavedFailed, ToastDuration.Long).Show();
            }
        }
    }
}
