using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public interface IPreferencesService
    {
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
    }
}
