#if ANDROID
using System.Security.Cryptography;
using System.Text;
using Android.Content;
using Android.Security.Keystore;
using Android.Views.TextService;
using AndroidX.Core.Content;
using Java.Lang;
using Java.Security;
using Java.Util.Concurrent;
using Javax.Crypto;
using Javax.Crypto.Spec;
using static AndroidX.Biometric.BiometricPrompt;
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
        private const string AndroidKeyStore = "AndroidKeyStore";

        private bool? _biometricsAvailable;

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

        public void Initialize(IContext context, string alias)
        {
            GenerateKey(alias);
            GenerateHmacKey(alias + "_HMAC");
            GenerateHmacKey(alias + "_FALLBACK" + "_HMAC");
        }

        public void GenerateKey(string alias)
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
                    .SetRandomizedEncryptionRequired(true)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                    .SetUserAuthenticationRequired(true)
                    .SetUserAuthenticationParameters(300, (int)(KeyPropertiesAuthType.BiometricStrong | KeyPropertiesAuthType.DeviceCredential))
                    .SetUserAuthenticationValidityDurationSeconds(-1); 


                keyGenerator.Init(builder.Build());
                keyGenerator.GenerateKey();
            }
        }

        public void GenerateHmacKey(string alias)
        {
            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmHmacSha256, AndroidKeyStore);

                var builder = new KeyGenParameterSpec.Builder(
                        alias,
                        KeyStorePurpose.Sign | KeyStorePurpose.Verify)
                    .SetDigests(KeyProperties.DigestSha256)
                    .SetUserAuthenticationRequired(false);

                keyGenerator.Init(builder.Build());
                keyGenerator.GenerateKey();
            }
        }

        public async Task StoreHmacAsync(IContext context, string alias, params byte[][] data)
        {
            byte[] hmac = ComputeHmac(context, alias, data);

            context?.Log("Storing HMAC");
            ISecureStorageService service = Services.GetService<ISecureStorageService>();
            await service.SaveAsync(alias + "_HMAC", Convert.ToBase64String(hmac));
        }

        public byte[] ComputeHmac(IContext context, string alias, params byte[][] data)
        {
            context?.Log("Computing HMAC");
            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            var hmacKey = keyStore.GetKey(alias + "_HMAC", null);

            var mac = Mac.GetInstance("HmacSHA256");
            mac.Init(hmacKey);

            byte[] hmacData = Array.Empty<byte>().With(data);
            byte[] encryptedHmac = mac.DoFinal(hmacData);
            return encryptedHmac;
        }

        public async Task GetAsync(IContext context, string alias, byte[] encryptedKey, byte[] iv, Action<string> onSuccess, Action<string> onError)
        {
            if (!BiometricsAvailable())
            {
                onError("No biometrics available");
                return;
            }

            context?.Log("Authenticating and decrypting database cipher");
            await AuthenticateAndDecryptKey(alias, encryptedKey, iv, onSuccess, onError);
        }

        public async Task SaveAsync(IContext context, string alias, string value)
        {
            if (!BiometricsAvailable())
                return;

            context?.Log("Storing key");
            await SaveKeyValue(context, alias, value);
        }

        private async Task SaveKeyValue(IContext context, string alias, string value)
        {
            context?.Log("Saving key against biometric data");
            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                return;
            }

            var secretKey = keyStore.GetKey(alias, null);

            var cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.EncryptMode, secretKey);

            byte[] encryptedKey = await PerformEncryptionWithAuth(cipher, value, alias);
            byte[] iv = cipher.GetIV();

            EncryptionContext encryptionContext = new(alias);

            encryptionContext.Add("_IV", iv);
            encryptionContext.Add("_KEY", encryptedKey);
            await encryptionContext.StoreAsync(context);
        }

        private async Task AuthenticateAndDecryptKey(
            string alias,
            byte[] encryptedKey,
            byte[] iv,
            Action<string> onSuccess,
            Action<string> onError)
        {
            Context context = Platform.CurrentActivity;
            var executor = ContextCompat.GetMainExecutor(Platform.CurrentActivity);
            var tcs = new TaskCompletionSource<bool>();

            var biometricPrompt = new BiometricPrompt(
                (AndroidX.Fragment.App.FragmentActivity)context,
                executor,
                new BiometricAuthCallback(
                    onSuccess: result =>
                    {
                        try
                        {
                            var cipher = result.CryptoObject.Cipher;
                            var decryptedKeyBytes = cipher.DoFinal(encryptedKey);
                            var decryptedKey = Convert.ToBase64String(decryptedKeyBytes);
                            onSuccess(decryptedKey);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            onError(ex.Message);
                            tcs.SetException(ex);
                        }
                    },
                    onError: msg =>
                    {
                        onError($"Auth error: {msg}");
                        tcs.SetException(new Exception(msg));
                    },
                    onFailed: () =>
                    {
                        onError("Authentication failed.");
                        tcs.SetException(new Exception());
                    }
                )
            );

            var keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            var secretKey = keyStore.GetKey(alias, null);

            var cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.DecryptMode, secretKey, new IvParameterSpec(iv));

            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle("Biometric authentication required")
                .SetSubtitle("Use fingerprint or face to unlock your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            await MainThread.InvokeOnMainThreadAsync(() =>
                biometricPrompt.Authenticate(promptInfo, new BiometricPrompt.CryptoObject(cipher)));

            await tcs.Task;

            // TODO: HMAC
        }

        private async Task<byte[]> PerformEncryptionWithAuth(Cipher cipher, string value, string alias) // TODO: merge with AuthenticateAndDecryptKey
        {
            byte[] encryptedKeyResult = null;
            var tcs = new TaskCompletionSource<bool>();

            var callback = new BiometricAuthCallback(
                onSuccess: result =>
                {
                    try
                    {
                        var authCipher = result.CryptoObject.Cipher;
                        encryptedKeyResult = authCipher.DoFinal(Convert.FromBase64String(value));
                        // Save to secure storage

                        //ISecureStorageService service = Services.GetService<ISecureStorageService>();
                        //SecureStorage.Default.SetAsync(alias, encryptedKey);
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                },
                onError: (error) => tcs.SetException(new Exception(error)),
                onFailed: (() => { }));

            Context context = Platform.CurrentActivity;
            var executor = ContextCompat.GetMainExecutor(Platform.CurrentActivity);

            var prompt = new BiometricPrompt(
                (AndroidX.Fragment.App.FragmentActivity)context,
                executor,
                callback);

            var promptInfo = new PromptInfo.Builder()
                .SetTitle("Authenticate Encryption")
                .SetSubtitle("Confirm biometrics to secure your key")
                .SetNegativeButtonText("Cancel")
                .Build();

            await MainThread.InvokeOnMainThreadAsync(() =>
                prompt.Authenticate(promptInfo, new BiometricPrompt.CryptoObject(cipher)));

            await tcs.Task;
            return encryptedKeyResult;
        }


    }
}

#endif
