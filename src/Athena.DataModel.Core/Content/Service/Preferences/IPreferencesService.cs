namespace Athena.DataModel.Core
{
    public interface IPreferencesService
    {
        public const int ToSVersion = 1;

        /// <summary>
        /// Gets whether the app is used for the first time.
        /// </summary>
        /// <returns></returns>
        bool IsFirstUsage();

        /// <summary>
        /// Gets the preference with the given <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        TResult Get<TResult>(string key, TResult defaultValue = default);

        /// <summary>
        /// Sets the preference with the provided <paramref name="key"/> to the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set<TValue>(string key, TValue value);

        /// <summary>
        /// Sets that the app was used for the first time.
        /// </summary>
        void SetFirstUsage();

        /// <summary>
        /// Gets the name of the user. 
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// Sets the name of the user.
        /// </summary>
        /// <param name="name"></param>
        void SetName(string name);

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <returns></returns>
        string GetLanguage();

        /// <summary>
        /// Sets the language.
        /// </summary>
        /// <param name="language"></param>
        void SetLanguage(string language);

        /// <summary>
        /// Gets the last used version.
        /// </summary>
        /// <returns></returns>
        int GetLastUsedVersion();

        /// <summary>
        /// Sets the last used version to the given <paramref name="version"/>.
        /// </summary>
        /// <param name="version"></param>
        void SetLastUsedVersion(int version);

        /// <summary>
        /// Gets the last seen ToS version.
        /// </summary>
        /// <returns></returns>
        int GetLastTermsOfUseVersion();

        /// <summary>
        /// Updates the last seen ToS version to the given <paramref name="version"/>.
        /// </summary>
        /// <param name="version"></param>
        void SetLastTermsOfUseVersion(int version);

        /// <summary>
        /// Gets whether the advanced scanner is used for the first time.
        /// </summary>
        /// <returns></returns>
        bool IsFirstScannerUsage();

        /// <summary>
        /// Sets whether the advanced scanner is used for the first time.
        /// </summary>
        void SetFirstScannerUsage();

        /// <summary>
        /// Gets whether the advanced scanner should be used.
        /// </summary>
        /// <returns></returns>
        bool GetUseAdvancedScanner();

        /// <summary>
        /// Sets whether the advanced scanner should be used.
        /// </summary>
        /// <param name="use"></param>
        void SetUseAdvancedScanner(bool use);

        /// <summary>
        /// Gets the <see cref="EncryptionMethod"/> that is used for de/encrypting data.
        /// </summary>
        /// <returns></returns>
        EncryptionMethod GetEncryptionMethod();

        /// <summary>
        /// Sets the <see cref="EncryptionMethod"/> that is used for de/encrypting data.
        /// </summary>
        /// <param name="method"></param>
        void SetEncryptionMethod(EncryptionMethod method);
    }
}
