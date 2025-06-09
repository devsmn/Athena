using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Athena.DataModel.Core
{
    public class DefaultDataEncryptionService : IDataEncryptionService
    {
        private readonly IBiometricKeyService _biometricKeyService;
        private readonly ISecureStorageService _secureStorageService;

        private const string DatabaseEncryptionFallbackSaltAlias = "fallback_dbencr_salt";
        private const string DatabaseEncryptionFallbackIvAlias = "fallback_dbencr_iv";
        private const string DatabaseEncryptionFallbackKeyAlias = "fallback_dbencr_key";
        private const string DatabaseEncryptionFallbackHmacHashAlias = "fallback_dbencr_hmac_hash";

        public DefaultDataEncryptionService()
        {
            _biometricKeyService = Services.GetService<IBiometricKeyService>();
            _secureStorageService = Services.GetService<ISecureStorageService>();
        }

        public void InitializeDatabaseCipher()
        {
            _biometricKeyService.PrepareDatabaseCipher();
        }

        public async Task SaveDatabaseCipher(string key, string fallbackPin)
        {
            await _biometricKeyService.SaveDatabaseEncryptionKeyAsync(key);
            await StoreEncryptedPinKeyAsync(key, fallbackPin);
        }

        public async Task<bool> ReadDatabaseCipherPrimary(Action<string> onSuccess, Action<string> onError)
        {
            string encryptedKeyBase64 = await _secureStorageService.GetAsync(DatabaseEncryptionFallbackKeyAlias);
            bool success = true;

            // Unfortunately, due to the java RT bindings, we cannot make the onSuccess and onError callbacks
            // asynchronous.
            // Therefore, the fallback has to be called explicitly if false is returned.
            _biometricKeyService.GetDatabaseEncryptionKey(
                encryptedKeyBase64,
                onSuccess,
                error =>
                {
                    onError(error);
                    success = false;
                });

            return success;
        }

        public async Task ReadDatabaseCipherFallback(string pin, Action<string> onSuccess, Action<string> onError)
        {
            string encodedKey = await RetrieveKeyWithPinAsync(pin);

            if (string.IsNullOrEmpty(encodedKey))
            {
                onError("Invalid pin");
                return;
            }

            onSuccess(encodedKey);
        }

        private async Task StoreEncryptedPinKeyAsync(string plainKey, string pin)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var derivedKey = DeriveKeyFromPin(pin, salt);

            using var aes = Aes.Create();
            aes.Key = derivedKey;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainKey), 0, plainKey.Length);

            byte[] hmacData = salt.With(iv, encrypted);
            var hmac = new HMACSHA256(derivedKey);
            var hmacHash = hmac.ComputeHash(hmacData);

            await _secureStorageService.SaveAsync(DatabaseEncryptionFallbackSaltAlias, Convert.ToBase64String(salt));
            await _secureStorageService.SaveAsync(DatabaseEncryptionFallbackIvAlias, Convert.ToBase64String(iv));
            await _secureStorageService.SaveAsync(DatabaseEncryptionFallbackKeyAlias, Convert.ToBase64String(encrypted));
            await _secureStorageService.SaveAsync(DatabaseEncryptionFallbackHmacHashAlias, Convert.ToBase64String(hmacHash));
        }

        private async Task<string> RetrieveKeyWithPinAsync(string pin)
        {
            string saltBase64 = await _secureStorageService.GetAsync(DatabaseEncryptionFallbackSaltAlias);
            string ivBase64 = await _secureStorageService.GetAsync(DatabaseEncryptionFallbackIvAlias);
            string encryptedBase64 = await _secureStorageService.GetAsync(DatabaseEncryptionFallbackKeyAlias);
            string hmacHashBase64 = await _secureStorageService.GetAsync(DatabaseEncryptionFallbackHmacHashAlias);

            if (string.IsNullOrEmpty(hmacHashBase64) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(ivBase64) || string.IsNullOrEmpty(encryptedBase64))
                return null;

            var salt = Convert.FromBase64String(saltBase64);
            var iv = Convert.FromBase64String(ivBase64);
            var encryptedKey = Convert.FromBase64String(encryptedBase64);
            var storedHmacHash = Convert.FromBase64String(hmacHashBase64);

            var derivedKey = DeriveKeyFromPin(pin, salt);

            // First, validate the integrity of the data.
            var hmac = new HMACSHA256(derivedKey);
            var hmacData = salt.With(iv, encryptedKey);
            var expectedHmacHash = hmac.ComputeHash(hmacData);

            bool integrityValid = expectedHmacHash.ConstantTimeIsEqualTo(storedHmacHash);

            if (!integrityValid)
            {
                // Integrity could not be validated. Either the PIN is incorrect, or the data is corrupted.
                return null;
            }

            try
            {

                using var aes = Aes.Create();
                aes.Key = derivedKey;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                using var decryptor = aes.CreateDecryptor();

                var decrypted = decryptor.TransformFinalBlock(encryptedKey, 0, encryptedKey.Length);
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
            var pbkdf2 = new Rfc2898DeriveBytes(pin, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keyLen);
        }

    }
}
