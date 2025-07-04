using System.Security.Cryptography;
using System.Text;

namespace Athena.DataModel.Core
{
    public class EncryptionContext
    {
        public string Alias { get; }
        public Dictionary<string, byte[]> Data { get; }

        public EncryptionContext(string alias)
        {
            Alias = alias;
            Data = new();
        }

        public async Task StoreAsync(IContext context)
        {
            IDataEncryptionService service = Services.GetService<IDataEncryptionService>();
            await service.StoreEncryption(context, this);
        }

        public async Task GetAsync(IContext context)
        {
            ISecureStorageService service = Services.GetService<ISecureStorageService>();
            string metaInfo = await service.GetAsync(Alias + "_META");

            if (string.IsNullOrEmpty(metaInfo))
                return;

            string[] keys = metaInfo.Split(";", StringSplitOptions.RemoveEmptyEntries);

            foreach (string dataKey in keys)
            {
                byte[] data = Convert.FromBase64String(await service.GetAsync(Alias + dataKey));
                Data.Add(dataKey, data);
            }
        }

        public void Add(string name, byte[] data)
        {
            Data.Add(name, data);
        }

        public bool IsIntegrityValid(IContext context)
        {
            byte[] storedHmac = Data["_HMAC"];
            IHardwareKeyStoreService storeService = Services.GetService<IHardwareKeyStoreService>();

            byte[] calculatedHmac = storeService.ComputeHmac(context, Alias, GetHmacData());
            return CryptographicOperations.FixedTimeEquals(storedHmac, calculatedHmac);
        }

        public byte[][] GetHmacData()
        {
            // Salt should not be included in the HMAC hash.
            return Data
                .Where(x => x.Key != "_SALT" && x.Key != "_HMAC")
                .Select(x => x.Value)
                .ToArray();
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

        public async Task<bool> ReadPrimaryAsync(
            IContext context,
            string alias,
            Action<string> onSuccess,
            Action<string> onError)
        {
            EncryptionContext encryptionContext = new(alias);
            await encryptionContext.GetAsync(context);
            bool success = true;

            // Unfortunately, due to the java RT bindings, we cannot make the onSuccess and onError callbacks
            // asynchronous.
            // Therefore, the fallback has to be called explicitly if false is returned.
            byte[] decryptedKey = await _hardwareKeyStoreService.GetAsync(
                context,
                alias,
                encryptionContext,
                error =>
                {
                    onError(error);
                    success = false;
                });

            if (decryptedKey != null)
            {
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

            using Aes aes = Aes.Create();
            aes.Key = derivedKey;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encryptedKey = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainKey), 0, plainKey.Length);

            string fallbackAlias = alias + "_FALLBACK";

            EncryptionContext encryptionContext = new(fallbackAlias);
            encryptionContext.Add("_SALT", salt);
            encryptionContext.Add("_IV", iv);
            encryptionContext.Add("_KEY", encryptedKey);
            await encryptionContext.StoreAsync(context);
        }

        public async Task StoreEncryption(IContext context, EncryptionContext encryptionContext)
        {
            context?.Log("Storing encrypted context");
            string metaInfo = string.Empty;

            foreach (KeyValuePair<string, byte[]> toStore in encryptionContext.Data)
            {
                metaInfo += $"{toStore.Key};";
                await _secureStorageService.SaveAsync(encryptionContext.Alias + toStore.Key, Convert.ToBase64String(toStore.Value));
            }

            await _hardwareKeyStoreService.StoreHmacAsync(context, encryptionContext.Alias, encryptionContext.GetHmacData());
            metaInfo += "_HMAC";
            await _secureStorageService.SaveAsync(encryptionContext.Alias + "_META", metaInfo);
        }

        private async Task<string> RetrieveKeyWithPinAsync(IContext context, string alias, string pin)
        {
            EncryptionContext encryptionContext = new(alias);
            await encryptionContext.GetAsync(context);

            byte[] salt = encryptionContext.Data["_SALT"];
            byte[] iv = encryptionContext.Data["_IV"];
            byte[] encryptedKey = encryptionContext.Data["_KEY"];
            byte[] hmacHash = encryptionContext.Data["_HMAC"];

            if (salt.IsNullOrEmpty() || iv.IsNullOrEmpty() || encryptedKey.IsNullOrEmpty() || hmacHash.IsNullOrEmpty())
                return null;

            if (!encryptionContext.IsIntegrityValid(context))
            {
                return null;
            }

            byte[] derivedKey = DeriveKeyFromPin(pin, salt);

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = derivedKey;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                using ICryptoTransform decryptor = aes.CreateDecryptor();

                byte[] decrypted = decryptor.TransformFinalBlock(encryptedKey, 0, encryptedKey.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                return null;
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
