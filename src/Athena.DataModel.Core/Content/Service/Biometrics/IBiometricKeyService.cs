using Android.Content;

namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides functionality to store crucial information protected by biometrics.
    /// </summary>
    public interface IBiometricKeyService
    {
        ///// <summary>
        ///// Prepares the database related AES key in the key store.
        ///// Only needs to be done when initializing the app for the first time
        ///// or when patching the app.
        ///// </summary>
        //void PrepareDatabaseCipher(IContext context);

        /// <summary>
        /// Generates an AES key.
        /// </summary>
        /// <param name="alias"></param>
        void GenerateKey(string alias);

        void Initialize(IContext context, string alias);

        /// <summary>
        /// Computes the HMAC for the given <paramref name="data"/>, using the hardware-backed provided AES key <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] ComputeHmac(IContext context, string alias, params byte[][] data);

        Task StoreHmacAsync(IContext context, string alias, params byte[][] data);


        /// <summary>
        /// Encrypts the given <paramref name="value"/> with the AES key stored in the android key store and
        /// saves it to the secure storage <see cref="ISecureStorageService"/>.
        /// </summary>
        /// <param name="value"></param>
        Task SaveAsync(IContext context, string alias, string value);

        /// <summary>
        /// Retrieves the database encryption key from <see cref="ISecureStorageService"/> and decodes it with the stored AES key.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        Task GetAsync(IContext context, string alias, byte[] encryptedKey, byte[] iv, Action<string> onSuccess, Action<string> onError);
    }
}
