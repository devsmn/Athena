using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    internal class DefaultPreferencesService : IPreferencesService
    {
        private const string FirstUsageKey = "FirstUsage";

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
    }
}
