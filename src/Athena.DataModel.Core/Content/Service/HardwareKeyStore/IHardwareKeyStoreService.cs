using Android.Graphics.Drawables;

namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides functionality to store crucial information protected by hardware-backed security.
    /// </summary>
    public interface IHardwareKeyStoreService
    {
        /// <summary>
        /// Generates an AES key.
        /// </summary>
        /// <param name="alias"></param>
        void GenerateKey(string alias);

        /// <summary>
        /// Initializes the AES key itself and the HMAC key for the given <paramref name="alias"/>,
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alias"></param>
        void Initialize(IContext context, string alias);

        /// <summary>
        /// Deletes the entries related to the given <paramref name="alias"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alias"></param>
        void Delete(IContext context, string alias);

        /// <summary>
        /// Computes the HMAC for the given <paramref name="data"/>, using the hardware-backed provided AES key <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] ComputeHmac(IContext context, string alias, params byte[][] data);

        /// <summary>
        /// Asynchronously stores the HMAC generated from the given <paramref name="data"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alias"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<byte[]> StoreHmacAsync(IContext context, string alias, params byte[][] data);

        /// <summary>
        /// Encrypts the given <paramref name="value"/> with the AES key stored in the android key store and
        /// saves it to the secure storage <see cref="ISecureStorageService"/>.
        /// </summary>
        /// <param name="value"></param>
        Task SaveAsync(IContext context, string alias, string value);

        /// <summary>
        /// Retrieves the database encryption key from <see cref="ISecureStorageService"/> and decodes it with the stored AES key.
        /// </summary>
        /// <param name="onError"></param>
        Task<byte[]> GetAsync(IContext context, string alias, EncryptionContext encryptionContext, Action<string> onError, Action onCancelled);

        /// <summary>
        /// Determines whether biometric authentication is available on this device.
        /// </summary>
        /// <returns></returns>
        bool BiometricsAvailable();
    }
}
