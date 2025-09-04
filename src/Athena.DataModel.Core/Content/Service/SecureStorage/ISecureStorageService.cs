namespace Athena.DataModel.Core
{
    public interface ISecureStorageService
    {
        /// <summary>
        /// Saves the given <paramref name="alias"/> with the provided <paramref name="key"/>.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task SaveAsync(string alias, string key);

        /// <summary>
        /// Gets the key of the given <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        Task<string> GetAsync(string alias);

        /// <summary>
        /// Deletes the given alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        void Delete(string alias);

        /// <summary>
        /// Generates a secure, random key.
        /// </summary>
        /// <returns></returns>
        string GenerateRandomKey();
    }
}
