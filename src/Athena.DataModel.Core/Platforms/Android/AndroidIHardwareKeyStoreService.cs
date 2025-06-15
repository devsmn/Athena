#if ANDROID
using System.Security.Cryptography;
using Android.App;
using Android.Content;
using Android.Security.Keystore;
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

        public BiometricAuthCallback(
            Action<AuthenticationResult> onSuccess,
            Action<string> onError,
            Action onFailed)
        {
            _onSuccess = onSuccess;
            _onError = onError;
            _onFailed = onFailed;
        }

        public override void OnAuthenticationSucceeded(AuthenticationResult result)
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

    /// <summary>
    /// Provides the android specific implementation of the <see cref="IHardwareKeyStoreService"/>.
    /// </summary>
    public class AndroidIHardwareKeyStoreService : IHardwareKeyStoreService
    {
        private const string AndroidKeyStore = "AndroidKeyStore";
        private bool? _biometricsAvailable;

        private bool BiometricsAvailable()
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
            GenerateHmacKey(alias + "_HMAC");
            GenerateHmacKey(alias + "_FALLBACK" + "_HMAC");
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
                    .SetUserAuthenticationRequired(true)
                    .SetUserAuthenticationParameters(300, (int)(KeyPropertiesAuthType.BiometricStrong | KeyPropertiesAuthType.DeviceCredential))
                    .SetUserAuthenticationValidityDurationSeconds(-1); 

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
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            IKey hmacKey = keyStore.GetKey(alias + "_HMAC", null);

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
            Action<string> onError)
        {
            if (!BiometricsAvailable())
            {
                onError("No biometrics available");
                return null;
            }

            byte[] iv = encryptionContext.Data["_IV"];
            byte[] encryptedKey = encryptionContext.Data["_KEY"];

            context?.Log("Authenticating and decrypting database cipher");
            KeyStore keyStore = KeyStore.GetInstance(AndroidKeyStore);
            keyStore.Load(null);
            IKey secretKey = keyStore.GetKey(alias, null);

            Cipher cipher = Cipher.GetInstance("AES/CBC/PKCS7Padding");
            cipher.Init(CipherMode.DecryptMode, secretKey, new IvParameterSpec(iv));

            PromptInfo promptInfo = new PromptInfo.Builder()
                .SetTitle("Biometric authentication required")
                .SetSubtitle("Use fingerprint or face to unlock your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            if (!encryptionContext.IsIntegrityValid(context))
            {
                return null;
            }

            return await RequestCipherExecution(promptInfo, cipher, encryptedKey);
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
                .SetSubtitle("Use fingerprint or face to unlock your data")
                .SetNegativeButtonText("Cancel")
                .Build();

            byte[] encryptedKey = await RequestCipherExecution(info, cipher, Convert.FromBase64String(value));
            byte[] iv = cipher.GetIV();

            EncryptionContext encryptionContext = new(alias);

            encryptionContext.Add("_IV", iv);
            encryptionContext.Add("_KEY", encryptedKey);
            await encryptionContext.StoreAsync(context);
        }

        private static async Task<byte[]> RequestCipherExecution(
            PromptInfo promptInfo,
            Cipher cipher,
            byte[] value) // TODO: merge with AuthenticateAndDecryptKey
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            BiometricAuthCallback callback = new BiometricAuthCallback(
                onSuccess: result =>
                {
                    try
                    {
                        Cipher authCipher = result.CryptoObject.Cipher;
                        tcs.SetResult(authCipher.DoFinal(value));
                        // Save to secure storage

                        //ISecureStorageService service = Services.GetService<ISecureStorageService>();
                        //SecureStorage.Default.SetAsync(alias, encryptedKey);
                        
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                },
                onError: error => tcs.SetException(new Exception(error)),
                onFailed: (() => tcs.SetResult(null)));

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
