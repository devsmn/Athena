#if ANDROID
using System.Security.Cryptography;
using System.Text;
using Android.Content;
using Android.Security.Keystore;
using AndroidX.Core.Content;
using Java.Lang;
using Java.Security;
using Javax.Crypto;
using BiometricManager = AndroidX.Biometric.BiometricManager;
using BiometricPrompt = AndroidX.Biometric.BiometricPrompt;
using CipherMode = Javax.Crypto.CipherMode;
using Exception = System.Exception;

namespace Athena.DataModel.Core.Platforms.Android
{
    public class BiometricAuthCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly Action<BiometricPrompt.AuthenticationResult> _onSuccess;
        private readonly Action<string> _onError;
        private readonly Action _onFailed;

        public BiometricAuthCallback(
            Action<BiometricPrompt.AuthenticationResult> onSuccess,
            Action<string> onError,
            Action onFailed)
        {
            _onSuccess = onSuccess;
            _onError = onError;
            _onFailed = onFailed;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            _onSuccess?.Invoke(result);
        }

        public override void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            base.OnAuthenticationError(errorCode, errString);
            _onError?.Invoke(errString?.ToString());
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            _onFailed?.Invoke();
        }
    }

    public class AndroidBiometricKeyService : IBiometricKeyService
    {
        private const string DatabaseEncryptionBiometricKeyAlias = "biometric_dbencr_key";
        private const string AndroidKeyStore = "AndroidKeyStore";

        private bool? _biometricsAvailable;

        public void PrepareDatabaseCipher()
        {
            GenerateKeyEntry(DatabaseEncryptionBiometricKeyAlias);
        }

        private bool BiometricsAvailable()
        {
            if (_biometricsAvailable != null)
                return _biometricsAvailable.Value;

            var context = Platform.CurrentActivity;

            if (context == null)
            {
                _biometricsAvailable = false;
            }
            else
            {
                var result = BiometricManager.From(context).CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);
                _biometricsAvailable = result == BiometricManager.BiometricSuccess;
            }

            return _biometricsAvailable.Value;
        }

        /// <summary>
        /// Creates an AES key in the android key store. This key can be used to encrypt other information
        /// which can then be stored in the preferences.
        /// </summary>
        /// <param name="alias"></param>
        private void GenerateKeyEntry(string alias)
        {
            if (!BiometricsAvailable())
                return;

            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, AndroidKeyStore);

                var builder = new KeyGenParameterSpec.Builder(
                        alias,
                        KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(KeyProperties.BlockModeCbc)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                    .SetUserAuthenticationParameters(300, (int)(KeyPropertiesAuthType.BiometricStrong | KeyPropertiesAuthType.DeviceCredential))
                    .SetUserAuthenticationRequired(true)
                    .SetUserAuthenticationValidityDurationSeconds(-1);

                keyGenerator.Init(builder.Build());
                keyGenerator.GenerateKey();
            }
        }

        public void GetDatabaseEncryptionKey(string encryptedKeyBase64, Action<string> onSuccess, Action<string> onError)
        {
            if (!BiometricsAvailable())
            {
                onError("No biometrics available");
                return;
            }

            AuthenticateAndDecryptKey(encryptedKeyBase64, onSuccess, onError);
        }

        private void AuthenticateAndDecryptKey(
            string encryptedKeyBase64,
            Action<string> onSuccess,
            Action<string> onError)
        {
            Context context = Platform.CurrentActivity;
            var executor = ContextCompat.GetMainExecutor(Platform.CurrentActivity);

            var biometricPrompt = new BiometricPrompt(
                (AndroidX.Fragment.App.FragmentActivity)context,
                executor,
                new BiometricAuthCallback(
                    onSuccess: result =>
                    {
                        try
                        {
                            var cipher = result.CryptoObject.Cipher;

                            var encryptedKey = Convert.FromBase64String(encryptedKeyBase64);
                            var decryptedKeyBytes = cipher.DoFinal(encryptedKey);
                            var decryptedKey = Encoding.UTF8.GetString(decryptedKeyBytes);
                            onSuccess(decryptedKey);
                        }
                        catch (Exception ex)
                        {
                            onError(ex.Message);
                        }
                    },
                    onError: msg =>
                    {
                        onError($"Auth error: {msg}");
                    },
                    onFailed: () =>
                    {
                        onError("Authentication failed.");
                    }
                )
            );

            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            var secretKey = (ISecretKey)keyStore.GetKey(DatabaseEncryptionBiometricKeyAlias, null);

            var cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.DecryptMode, secretKey);

            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle("Biometric authentication required")
                .SetSubtitle("Use fingerprint or face to unlock your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            biometricPrompt.Authenticate(promptInfo, new BiometricPrompt.CryptoObject(cipher));
        }

        public async Task SaveDatabaseEncryptionKeyAsync(string key)
        {
            if (!BiometricsAvailable())
                return;

            await SaveKeyValue(DatabaseEncryptionBiometricKeyAlias, key);
        }

        private async Task SaveKeyValue(string keyAlias, string plainKey)
        {
            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            var secretKey = (ISecretKey)keyStore.GetKey(keyAlias, null);

            var cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.EncryptMode, secretKey);
            var iv = cipher.GetIV();

            var encryptedBytes = cipher.DoFinal(Encoding.UTF8.GetBytes(plainKey));
            var encryptedKey = Convert.ToBase64String(encryptedBytes);

            var service = Services.GetService<ISecureStorageService>();
            await service.SaveAsync(keyAlias, encryptedKey);
        }
    }
}

#endif
