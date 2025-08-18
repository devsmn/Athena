using Android.Telephony;
using Athena.Data.Core;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Athena.DataModel.Core;
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
            IContext context = RetrieveContext();
            IBackupService backupService = Services.GetService<IBackupService>();

            PickOptions options = new();
            options.PickerTitle = "Select the backup to restore";
            var result = await FilePicker.PickAsync(options);

            if (result != null)
            {
                BackupRestoreResult restoreResult = await backupService.Restore(
                    context,
                    result.FullPath,
                    RequireUserConfirmation);

                if (restoreResult.IsSuccess)
                {
                    Application.Current.Windows[0].Page = new ContainerPage();
                    await Toast.Make("Successfully restored backup", ToastDuration.Long).Show();
                }
                else
                {
                    if (restoreResult.Code == BackupRestoreResultCode.AbortedByUser)
                    {
                        await Toast.Make("Cancelled restoring backup", ToastDuration.Long).Show();
                        return;
                    }

                    await DisplayAlert(
                        "Error while restoring backup",
                        $"{restoreResult.Code} - {restoreResult.ErrorInfo}",
                        "ok",
                        "ok");
                }
            }
            else
            {
                await Toast.Make("Invalid file selected", ToastDuration.Long).Show();
            }
        }

        private async Task<bool> RequireUserConfirmation(string msg, string okButton, string cancelButton)
        {
            return await DisplayAlert("Restore backup", msg, okButton, cancelButton);
        }

        [RelayCommand]
        private async Task CreateBackup()
        {
            IContext context = RetrieveContext();

            bool showKey = await DisplayAlert(
                "Encryption key",
                $"Do you want to view the encryption key used for this backup?{Environment.NewLine}" +
                "This is required if you plan to restore the backup on a different device or after reinstalling the app. " +
                $"It is not needed if you restore the backup on this device without uninstalling.{Environment.NewLine}" +
                "Please note that the key is not your password, as the key was generated automatically.",
                "Show key",
                "Skip");

            if (showKey)
            {
                string cipher = string.Empty;

                SqliteProxy sqlProxy = new();
                IDataProviderAuthenticator sqlAuth = sqlProxy.RequestAuthenticator();
                IDataEncryptionService encryptionService = Services.GetService<IDataEncryptionService>();
                bool primarySucceeded = await encryptionService.ReadPrimaryAsync(context, IDataEncryptionService.DatabaseAlias, (c) => cipher = c, _ => { });

                if (!primarySucceeded || !await sqlAuth.AuthenticateAsync(cipher))
                {
                    IPasswordService passwordService = Services.GetService<IPasswordService>();

                    bool firstTry = true;

                    do
                    {
                        context.Log("Requesting fallback access to database");
                        string pin = string.Empty;
                        await passwordService.Prompt(context, !firstTry, (str) => pin = str);

                        await encryptionService.ReadFallbackAsync(
                            context,
                            IDataEncryptionService.DatabaseAlias,
                            pin, key => cipher = key,
                            error => context.Log(error));

                        firstTry = false;

                    } while (await sqlAuth.AuthenticateAsync(cipher) == false);
                }

                bool copy = await DisplayAlert(
                    "Show key",
                    $"Encryption key: {Environment.NewLine}{Environment.NewLine}" +
                    $"{cipher}{Environment.NewLine}{Environment.NewLine}" +
                    $"Please store the key in a secure location.",
                    "Copy key",
                    "Close");

                if (copy)
                {
                    await Clipboard.SetTextAsync(cipher);
                    await Toast.Make("Successfully copied key to clipboard", ToastDuration.Long).Show();
                }

                cipher = null;
            }

            IBackupService backupService = Services.GetService<IBackupService>();
            FileSaverResult? result = await backupService.Create(context);

            if (result?.IsSuccessful == true)
            {
                await Toast.Make($"Successfully saved backup", ToastDuration.Long).Show();
            }
            else
            {
                if (result?.Exception != null)
                    context.Log(result.Exception);

                await Toast.Make("Unable to save backup", ToastDuration.Long).Show();
            }
        }
    }
}
