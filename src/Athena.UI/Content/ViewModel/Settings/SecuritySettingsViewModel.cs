using Athena.Data.Core;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    internal partial class SecuritySettingsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _biometricsAvailable;

        [ObservableProperty]
        private bool _useBiometrics;

        private readonly IPasswordService _passwordService;
        private readonly IDataEncryptionService _encryptionService;
        private readonly IPreferencesService _prefService;

        public SecuritySettingsViewModel()
        {
            IHardwareKeyStoreService hardwareKeyStoreService = Services.GetService<IHardwareKeyStoreService>();
            _passwordService = Services.GetService<IPasswordService>();
            _encryptionService = Services.GetService<IDataEncryptionService>();
            _prefService = Services.GetService<IPreferencesService>();

            BiometricsAvailable = hardwareKeyStoreService.BiometricsAvailable();

            if (BiometricsAvailable)
                UseBiometrics = _prefService.GetEncryptionMethod() == EncryptionMethod.Biometrics;
        }

        [RelayCommand]
        private void UseBiometricsChanged()
        {
            _prefService.SetEncryptionMethod(UseBiometrics ? EncryptionMethod.Biometrics : EncryptionMethod.Password);
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            IContext context = RetrieveReportContext();
            IDataProviderAuthenticator sqlAuth = new SqliteProxy().RequestAuthenticator();

            context.Log("Reading cipher");

            string oldAlias = await _encryptionService.GetActiveAliasAsync();

            // First, read the cipher via the encryption sources.
            string cipher = string.Empty;
            bool primarySucceeded = await _encryptionService.ReadPrimaryAsync(context, oldAlias, (c) => cipher = c, context.Log, () => { });

            if (!primarySucceeded || !await sqlAuth.AuthenticateAsync(cipher))
            {
                bool firstTry = true;

                do
                {
                    string pin = string.Empty;
                    bool cancelled = false;

                    await _passwordService.Prompt(
                        context,
                        Localization.PasswordUnlockData,
                        !firstTry,
                        (str) => pin = str,
                        () => cancelled = true);

                    if (cancelled)
                    {
                        await Toast.Make("Password change cancelled", ToastDuration.Long).Show();
                        return;
                    }

                    await _encryptionService.ReadFallbackAsync(
                        context,
                        oldAlias,
                        pin, key => cipher = key,
                        context.Log);

                    firstTry = false;

                } while (!await sqlAuth.AuthenticateAsync(cipher));
            }

            IsBusy = true;

            // Cipher is valid, password and biometrics can be replaced.
            context.Log("Storing new encryption");

            string pw = string.Empty;
            await _passwordService.New(context, userPw => pw = userPw);
            string newAlias = _encryptionService.GenerateNewAlias();

            try
            {
                _encryptionService.Initialize(context, newAlias);
                if (!await _encryptionService.SaveAsync(context, newAlias, cipher, pw))
                {
                    await Toast.Make("Failed to change password", ToastDuration.Long).Show();
                    return;
                }
                await _encryptionService.SaveNewAliasAsync(newAlias);
                await _encryptionService.DeleteAsync(context, oldAlias);
                await Toast.Make("Password changed", ToastDuration.Long).Show();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
