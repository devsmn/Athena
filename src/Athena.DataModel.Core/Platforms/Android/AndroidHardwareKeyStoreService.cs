#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using Android.Systems;
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
    public class BiometricAuthCallback : AuthenticationCallback
    {
        private readonly Action<AuthenticationResult> _onSuccess;
        private readonly Action<string> _onError;
        private readonly Action _onFailed;
        private readonly Action _onCancelled;

        public BiometricAuthCallback(
            Action<AuthenticationResult> onSuccess,
            Action<string> onError,
            Action onFailed,
            Action onCancelled)
        {
            _onSuccess = onSuccess;
            _onError = onError;
            _onFailed = onFailed;
            _onCancelled = onCancelled;
        }

        public override void OnAuthenticationSucceeded(AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            _onSuccess?.Invoke(result);
        }

        public override void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            if (errorCode == BiometricPrompt.ErrorUserCanceled
                || errorCode == BiometricPrompt.ErrorCanceled
                || errorCode == BiometricPrompt.ErrorNegativeButton)
            {
                _onCancelled?.Invoke();
                return;
            }

            base.OnAuthenticationError(errorCode, errString);
            _onError?.Invoke(errString?.ToString());
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            _onFailed?.Invoke();
        }
    }

    /// <summary>
    /// Provides the android specific implementation of the <see cref="IHardwareKeyStoreService"/>.
    /// </summary>
    public class AndroidHardwareKeyStoreService : IHardwareKeyStoreService
    {
        private class RequestCipherExecutionResult
        {
            public byte[] Data { get; set; }
            public Exception Exception { get; set; }
            public bool Cancelled { get; set; }
            public bool Success => Exception == null && !Cancelled;
        }

        private const string AndroidKeyStore = "AndroidKeyStore";
        private bool? _biometricsAvailable;

        public bool BiometricsAvailable()
        {
            if (_biometricsAvailable != null)
                return _biometricsAvailable.Value;

            Activity context = Platform.CurrentActivity;

            if (context == null)
            {
                _biometricsAvailable = false;
            }
            else
            {
                int result = BiometricManager.From(context).CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);
                _biometricsAvailable = result == BiometricManager.BiometricSuccess;
            }

            return _biometricsAvailable.Value;
        }

        public void Initialize(IContext context, string alias)
        {
            GenerateKey(alias);
            GenerateHmacKey(alias + EncryptionContext.Hmac);
            GenerateHmacKey(alias + "_FALLBACK" + EncryptionContext.Hmac);
        }

        public void Delete(IContext context, string alias)
        {
            if (!BiometricsAvailable())
                return;

            try
            {
                KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
                keyStore.Load(null);
                DeleteEntryCore(keyStore, alias);
                DeleteEntryCore(keyStore, alias + EncryptionContext.Hmac);
                DeleteEntryCore(keyStore, alias + "_FALLBACK" + EncryptionContext.Hmac);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        private void DeleteEntryCore(KeyStore store, string alias)
        {
            if (!store.ContainsAlias(alias))
                return;

            store.DeleteEntry(alias);
        }

        public void GenerateKey(string alias)
        {
            if (!BiometricsAvailable())
                return;

            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                KeyGenerator keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, AndroidKeyStore);

                KeyGenParameterSpec.Builder builder = new KeyGenParameterSpec.Builder(
                       alias,
                       KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                   .SetBlockModes(KeyProperties.BlockModeCbc)
                   .SetRandomizedEncryptionRequired(true)
                   .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                   .SetUserAuthenticationValidityDurationSeconds(-1);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // API 30+
                {
                    builder.SetUserAuthenticationParameters(300, (int)(KeyPropertiesAuthType.BiometricStrong | KeyPropertiesAuthType.DeviceCredential));
                }
                else
                {
                    builder.SetUserAuthenticationRequired(true);
                }


                keyGenerator.Init(builder.Build());
                keyGenerator.GenerateKey();
            }
        }

        public void GenerateHmacKey(string alias)
        {
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                KeyGenerator keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmHmacSha256, AndroidKeyStore);

                KeyGenParameterSpec.Builder builder = new KeyGenParameterSpec.Builder(
                        alias,
                        KeyStorePurpose.Sign | KeyStorePurpose.Verify)
                    .SetDigests(KeyProperties.DigestSha256)
                    .SetUserAuthenticationRequired(false);

                keyGenerator.Init(builder.Build());
                keyGenerator.GenerateKey();
            }
        }

        public async Task<byte[]> StoreHmacAsync(IContext context, string alias, params byte[][] data)
        {
            byte[] hmac = ComputeHmac(context, alias, data);

            context?.Log("Storing HMAC");
            ISecureStorageService service = Services.GetService<ISecureStorageService>();
            await service.SaveAsync(alias + EncryptionContext.Hmac, Convert.ToBase64String(hmac));
            return hmac;
        }

        public byte[] ComputeHmac(IContext context, string alias, params byte[][] data)
        {
            context?.Log("Computing HMAC");
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            IKey hmacKey = keyStore.GetKey(alias + EncryptionContext.Hmac, null);

            Mac mac = Mac.GetInstance("HmacSHA256");
            mac.Init(hmacKey);

            byte[] hmacData = Array.Empty<byte>().With(data);
            byte[] encryptedHmac = mac.DoFinal(hmacData);
            return encryptedHmac;
        }

        public async Task<byte[]> GetAsync(
            IContext context,
            string alias,
            EncryptionContext encryptionContext,
            Action<string> onError,
            Action onCancelled)
        {
            if (!BiometricsAvailable())
            {
                onError("No biometrics available");
                return null;
            }

            byte[] iv = encryptionContext.GetData(EncryptionContext.Iv);
            byte[] encryptedKey = encryptionContext.GetData(EncryptionContext.Key);

            if (iv.IsNullOrEmpty() || encryptedKey.IsNullOrEmpty())
            {
                onError("IV or key invalid");
                return null;
            }

            context?.Log("Authenticating and decrypting database cipher");
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            IKey secretKey = keyStore.GetKey(alias, null);

            Cipher cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.DecryptMode, secretKey, new IvParameterSpec(iv));

            PromptInfo promptInfo = new PromptInfo.Builder()
                .SetTitle("Biometric authentication required")
                .SetSubtitle("Authenticate to unlock your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            if (!encryptionContext.IsIntegrityValid(context))
            {
                return null;
            }

            var res = await RequestCipherExecution(promptInfo, cipher, encryptedKey);

            if (res.Success)
                return res.Data;

            if (res.Exception != null)
            {
                context?.Log(res.Exception);
                return null;
            }

            if (res.Cancelled)
            {
                context?.Log("Cancelled by user");
                onCancelled();
            }

            return null;
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
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(alias))
            {
                return;
            }

            IKey secretKey = keyStore.GetKey(alias, null);

            Cipher cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.EncryptMode, secretKey);

            PromptInfo info = new PromptInfo.Builder()
                .SetTitle("Biometric authentication required")
                .SetSubtitle("Authenticate to save your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            byte[] encryptedKey = null;
            var res = await RequestCipherExecution(info, cipher, Convert.FromBase64String(value));

            if (res.Success)
                encryptedKey = res.Data;

            if (res.Exception != null)
            {
                context?.Log(res.Exception);
                return;
            }

            if (res.Cancelled)
            {
                context?.Log("Cancelled by user");
                return;
            }

            byte[] iv = cipher.GetIV();

            EncryptionContext encryptionContext = new(alias);

            encryptionContext.Add(EncryptionContext.Iv, iv);
            encryptionContext.Add(EncryptionContext.Key, encryptedKey);
            await encryptionContext.StoreAsync(context);
        }

        private static async Task<RequestCipherExecutionResult> RequestCipherExecution(
            PromptInfo promptInfo,
            Cipher cipher,
            byte[] value)
        {
            TaskCompletionSource<RequestCipherExecutionResult> tcs = new TaskCompletionSource<RequestCipherExecutionResult>();
            RequestCipherExecutionResult cipherRes = new();

            BiometricAuthCallback callback = new BiometricAuthCallback(
                onSuccess: result =>
                {
                    try
                    {
                        Cipher authCipher = result.CryptoObject.Cipher;
                        cipherRes.Data = authCipher.DoFinal(value);
                        tcs.SetResult(cipherRes);
                    }
                    catch (Exception ex)
                    {
                        cipherRes.Exception = ex;
                        tcs.SetResult(cipherRes);
                    }
                },
                onCancelled: (() =>
                {
                    cipherRes.Cancelled = true;
                    tcs.SetResult(cipherRes);
                }),
                onError: error =>
                {
                    cipherRes.Exception = new Exception(error);
                    tcs.SetResult(cipherRes);
                },
                onFailed: (() =>
                {
                    cipherRes.Exception = new Exception("Failed");
                    tcs.SetResult(cipherRes);
                }));

            Context context = Platform.CurrentActivity;
            IExecutor executor = ContextCompat.GetMainExecutor(Platform.CurrentActivity);

            BiometricPrompt prompt = new BiometricPrompt(
                (AndroidX.Fragment.App.FragmentActivity)context,
                executor,
                callback);

            await MainThread.InvokeOnMainThreadAsync(() =>
                prompt.Authenticate(promptInfo, new CryptoObject(cipher)));

            return await tcs.Task;
        }
    }
}

#endif
