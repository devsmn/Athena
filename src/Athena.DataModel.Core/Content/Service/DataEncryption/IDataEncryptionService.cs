namespace Athena.DataModel.Core
{
    public interface IDataEncryptionService
    {
        public const string DatabaseAlias = "DATA_ENCRYPTION_BIOMETRICS";

        /// <summary>
        /// Initializes the context needed for encrypting the given <paramref name="alias"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alias"></param>
        void Initialize(IContext context, string alias);

        /// <summary>
        /// Saves the given value.
        /// <para>
        /// The value is saved twice: Secured with the primary authorization (biometrics) and as a fallback using the <paramref name="fallbackPin"/>
        /// </para>
        /// 
        /// <para>
        /// Biometrics:
        /// The key is secured using a hardware-backed AES key from the Android Key Store. To access the AES key, user authentication is required.
        /// The AES key is used to encrypt the given <paramref name="value"/>. Before storing the encrypted key, the HMAC is generated.
        /// The HMAC is based on a hardware-backed AES key from the Android Key Store. In this case, no used authentication is required to access the HMAC key.
        /// The encrypted value is then stored along with the related HMAC in the secure storage.
        /// </para>
        /// 
        /// <para>Fallback:
        /// </para>
        /// The key is secured using an AES key derived from the given <paramref name="fallbackPin"/>.
        /// The AES key is used to encrypt the given <paramref name="value"/>. Before storing the encrypted key, the HMAC is generated.
        /// The HMAC is based on a hardware-backed AES key from the Android Key Store. In this case, no used authentication is required to access the HMAC key.
        /// The encrypted value is then stored along with the related HMAC in the secure storage.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fallbackPin"></param>
        /// <returns></returns>
        Task SaveAsync(IContext context, string alias, string value, string fallbackPin);

        /// <summary>
        /// Reads the database cipher while authorizing against the primary source (biometrics).
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        Task<bool> ReadPrimaryAsync(IContext context, string alias, Action<string> onSuccess, Action<string> onError);


        /// <summary>
        /// Reads the database cipher while validating against the fallback source (PIN).
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        Task ReadFallbackAsync(IContext context, string alias, string pin ,Action<string> onSuccess, Action<string> onError);

        /// <summary>
        /// Stores the encryption with the given parameters, including the HMAC hash.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encryptionContext"></param>
        /// <returns></returns>
        Task StoreEncryption(IContext context, EncryptionContext encryptionContext);
    }
}
