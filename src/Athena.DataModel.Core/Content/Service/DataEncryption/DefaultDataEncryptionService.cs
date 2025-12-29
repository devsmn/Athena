using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Athena.DataModel.Core
{
    public class EncryptionContext
    {
        public const string Iv = "_IV";
        public const string Meta = "_META";
        public const string Hmac = "_HMAC";
        public const string Salt = "_SALT";
        public const string Key = "_KEY";

        public string Alias { get; }
        private readonly Dictionary<string, byte[]> _data;

        public EncryptionContext(string alias)
        {
            Alias = alias;
            _data = new();
        }

        public async Task StoreAsync(IContext context)
        {
            IDataEncryptionService service = Services.GetService<IDataEncryptionService>();
            await service.StoreEncryption(context, this);
        }

        public async Task GetAsync(IContext context)
        {
            ISecureStorageService service = Services.GetService<ISecureStorageService>();
            string metaInfo = await service.GetAsync(Alias + Meta);

            if (string.IsNullOrEmpty(metaInfo))
                return;

            string[] keys = metaInfo.Split(";", StringSplitOptions.RemoveEmptyEntries);

            foreach (string dataKey in keys)
            {
                byte[] data = Convert.FromBase64String(await service.GetAsync(Alias + dataKey));
                _data.Add(dataKey, data);
            }
        }

        public async Task DeleteAsync(IContext context)
        {
            ISecureStorageService service = Services.GetService<ISecureStorageService>();
            string metaInfo = await service.GetAsync(Alias + Meta);

            if (string.IsNullOrEmpty(metaInfo))
                return;

            string[] keys = metaInfo.Split(";", StringSplitOptions.RemoveEmptyEntries);

            foreach (string dataKey in keys)
            {
                service.Delete(Alias + dataKey);
            }
        }

        public void Add(string name, byte[] data)
        {
            _data.Add(name, data);
        }

        public bool IsIntegrityValid(IContext context)
        {
            byte[] storedHmac = GetData(Hmac);
            IHardwareKeyStoreService storeService = Services.GetService<IHardwareKeyStoreService>();

            byte[] calculatedHmac = storeService.ComputeHmac(context, Alias, GetHmacData());
            return CryptographicOperations.FixedTimeEquals(storedHmac, calculatedHmac);
        }

        public byte[][] GetHmacData()
        {
            // Salt should not be included in the HMAC hash.
            return _data
                .Where(x => x.Key != Salt && x.Key != Hmac)
                .Select(x => x.Value)
                .ToArray();
        }

        public byte[]? GetData(string key)
        {
            return _data.GetValueOrDefault(key);
        }

        public IEnumerable<KeyValuePair<string, byte[]>> EnumerateData()
        {
            foreach (var data in _data)
                yield return data;
        }
    }

    public class DefaultDataEncryptionService : IDataEncryptionService
    {
        private readonly IHardwareKeyStoreService _hardwareKeyStoreService;
        private readonly ISecureStorageService _secureStorageService;

        public DefaultDataEncryptionService()
        {
            _hardwareKeyStoreService = Services.GetService<IHardwareKeyStoreService>();
            _secureStorageService = Services.GetService<ISecureStorageService>();
        }

        public void Initialize(IContext context, string alias)
        {
            _hardwareKeyStoreService.Initialize(context, alias);
        }

        public async Task SaveAsync(IContext context, string alias, string value, string fallbackPin)
        {
            await _hardwareKeyStoreService.SaveAsync(context, alias, value);
            await StoreFallbackKeyAsync(context, alias, value, fallbackPin);
        }

        public async Task DeleteAsync(IContext context, string alias)
        {
            _hardwareKeyStoreService.Delete(context, alias);

            EncryptionContext encryptionContext = new(alias);
            await encryptionContext.DeleteAsync(context);

            await DeleteFallbackAsync(context, alias);
        }

        public async Task DeleteFallbackAsync(IContext context, string alias)
        {
            EncryptionContext encryptionContext = new(alias + "_FALLBACK");
            await encryptionContext.DeleteAsync(context);
        }

        public async Task<bool> ReadPrimaryAsync(
            IContext context,
            string alias,
            Action<string> onSuccess,
            Action<string> onError,
            Action onCancelled)
        {
            EncryptionContext encryptionContext = new(alias);
            await encryptionContext.GetAsync(context);
            bool success = false;

            // Unfortunately, due to the java RT bindings, we cannot make the onSuccess and onError callbacks
            // asynchronous.
            // Therefore, the fallback has to be called explicitly if false is returned.
            byte[] decryptedKey = await _hardwareKeyStoreService.GetAsync(
                context,
                alias,
                encryptionContext,
                onError,
                onCancelled);

            if (decryptedKey != null)
            {
                success = true;
                onSuccess(Convert.ToBase64String(decryptedKey));
            }

            return success;
        }

        public async Task ReadFallbackAsync(
            IContext context,
            string alias,
            string pin,
            Action<string> onSuccess,
            Action<string> onError)
        {
            string fallbackAlias = alias + "_FALLBACK";
            string encodedKey = await RetrieveKeyWithPinAsync(context, fallbackAlias, pin);

            if (string.IsNullOrEmpty(encodedKey))
            {
                onError("Invalid pin");
                return;
            }

            onSuccess(encodedKey);
        }

        private async Task StoreFallbackKeyAsync(IContext context, string alias, string plainKey, string pin)
        {
            context?.Log("Generating fallback authentication");
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] derivedKey = DeriveKeyFromPin(pin, salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = derivedKey;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                byte[] iv = aes.IV;
                byte[] encryptedKey;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedKey = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainKey), 0, plainKey.Length);
                }

                string fallbackAlias = alias + "_FALLBACK";

                EncryptionContext encryptionContext = new(fallbackAlias);
                encryptionContext.Add(EncryptionContext.Salt, salt);
                encryptionContext.Add(EncryptionContext.Iv, iv);
                encryptionContext.Add(EncryptionContext.Key, encryptedKey);

                await encryptionContext.StoreAsync(context);
            }
        }

        public async Task StoreEncryption(IContext context, EncryptionContext encryptionContext)
        {
            context?.Log("Storing encrypted context");
            string metaInfo = string.Empty;

            foreach (KeyValuePair<string, byte[]> toStore in encryptionContext.EnumerateData())
            {
                metaInfo += $"{toStore.Key};";
                await _secureStorageService.SaveAsync(encryptionContext.Alias + toStore.Key, Convert.ToBase64String(toStore.Value));
            }

            await _hardwareKeyStoreService.StoreHmacAsync(context, encryptionContext.Alias, encryptionContext.GetHmacData());
            metaInfo += EncryptionContext.Hmac;
            await _secureStorageService.SaveAsync(encryptionContext.Alias + EncryptionContext.Meta, metaInfo);
        }

        private async Task<string> RetrieveKeyWithPinAsync(IContext context, string alias, string pin)
        {
            EncryptionContext encryptionContext = new(alias);
            await encryptionContext.GetAsync(context);

            byte[] salt = encryptionContext.GetData(EncryptionContext.Salt);
            byte[] iv = encryptionContext.GetData(EncryptionContext.Iv);
            byte[] encryptedKey = encryptionContext.GetData(EncryptionContext.Key);
            byte[] hmacHash = encryptionContext.GetData(EncryptionContext.Hmac);

            if (salt.IsNullOrEmpty() || iv.IsNullOrEmpty() || encryptedKey.IsNullOrEmpty() || hmacHash.IsNullOrEmpty())
                return null;

            if (!encryptionContext.IsIntegrityValid(context))
            {
                return null;
            }

            byte[] derivedKey = DeriveKeyFromPin(pin, salt);

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = derivedKey;
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;
                    byte[] decrypted = null;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        decrypted = decryptor.TransformFinalBlock(encryptedKey, 0, encryptedKey.Length);
                    }

                    return Encoding.UTF8.GetString(decrypted);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return null;
        }

        private byte[] DeriveKeyFromPin(string pin, byte[] salt, int iterations = 10000, int keyLen = 32)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(pin, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keyLen);
        }

    }
}
