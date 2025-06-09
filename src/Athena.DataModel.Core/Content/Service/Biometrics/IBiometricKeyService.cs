using Android.Content;

namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides functionality to store crucial information protected by biometrics.
    /// </summary>
    public interface IBiometricKeyService
    {
        /// <summary>
        /// Prepares the database related AES key in the key store.
        /// Only needs to be done when initializing the app for the first time
        /// or when patching the app.
        /// </summary>
        void PrepareDatabaseCipher();

        /// <summary>
        /// Encrypts the given <paramref name="key"/> with the AES key stored in the android key store and
        /// saves it to the secure storage <see cref="ISecureStorageService"/>.
        /// </summary>
        /// <param name="key"></param>
        Task SaveDatabaseEncryptionKeyAsync(string key);

        /// <summary>
        /// Retrieves the database encryption key from <see cref="ISecureStorageService"/> and decodes it with the stored AES key.
        /// </summary>
        /// <param name="encryptedKeyBase64"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        void GetDatabaseEncryptionKey(string encryptedKeyBase64, Action<string> onSuccess, Action<string> onError);
    }
}
