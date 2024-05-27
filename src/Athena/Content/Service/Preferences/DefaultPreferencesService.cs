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


        public bool IsFirstUsage()
        {
            return Get(FirstUsageKey, true);
        }

        public TResult Get<TResult>(string key, TResult defaultValue = default)
        {
            return Preferences.Default.Get(key, defaultValue);
        }

        public void SetFirstUsage()
        {
            Preferences.Default.Set(FirstUsageKey, false);
        }
    }
}
