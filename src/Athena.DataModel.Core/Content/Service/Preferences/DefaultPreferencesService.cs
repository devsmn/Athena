namespace Athena.DataModel.Core
{
    /// <summary>
    /// The default implementation of the <see cref="IPreferencesService"/>.
    /// </summary>
    public class DefaultPreferencesService : IPreferencesService
    {
        private const string FirstUsageKey = "FirstUsage";
        private const string NameKey = "Name";
        private const string LanguageKey = "Language";
        private const string DefaultLanguage = "en-US";
        private const string LastUsedVersion = "LastUsedVersion";
        private const string LastTermsVersion = "LastToSVersion";
        private const string FirstScannerUsageKey = "FirstUsageScanner";
        private const string UseAdvancedScannerKey = "UseAdvancedScanner";
        private const string EncryptionMethodKey = "EncryptionMethod";

        /// <inheritdoc />  
        public bool IsFirstUsage()
        {
            return Get(FirstUsageKey, true);
        }

        /// <inheritdoc />  
        public TResult Get<TResult>(string key, TResult defaultValue = default)
        {
            return Preferences.Default.Get(key, defaultValue);
        }

        /// <inheritdoc />  
        public void Set<TValue>(string key, TValue value)
        {
            Preferences.Default.Set(key, value);
        }

        /// <inheritdoc />  
        public void SetFirstUsage()
        {
            Set(FirstUsageKey, false);
        }

        /// <inheritdoc />  
        public string GetName()
        {
            return Get(NameKey, string.Empty);
        }

        /// <inheritdoc />  
        public void SetName(string name)
        {
            Set(NameKey, name);
        }

        /// <inheritdoc />  
        public string GetLanguage()
        {
            return Get(LanguageKey, DefaultLanguage);
        }

        /// <inheritdoc />  
        public void SetLanguage(string language)
        {
            Set(LanguageKey, language);
        }

        public int GetLastUsedVersion()
        {
            return Get(LastUsedVersion, 0);
        }

        public void SetLastUsedVersion(int version)
        {
            Set(LastUsedVersion, version);
        }

        public int GetLastTermsOfUseVersion()
        {
            return Get(LastTermsVersion, 0);
        }

        public void SetLastTermsOfUseVersion(int version)
        {
            Set(LastTermsVersion, version);
        }

        public bool IsFirstScannerUsage()
        {
            return Get(FirstScannerUsageKey, true);
        }

        public void SetFirstScannerUsage()
        {
            Set(FirstScannerUsageKey, false);
        }

        public bool GetUseAdvancedScanner()
        {
            return Get(UseAdvancedScannerKey, false);
        }

        public void SetUseAdvancedScanner(bool use)
        {
            Set(UseAdvancedScannerKey, use);
        }

        public EncryptionMethod GetEncryptionMethod()
        {
            return (EncryptionMethod)Get(EncryptionMethodKey, (int)EncryptionMethod.Undefined);
        }

        public void SetEncryptionMethod(EncryptionMethod method)
        {
            Set(EncryptionMethodKey, (int)method);
        }
    }
}
