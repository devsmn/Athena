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
        bool IsFirstUsage();
        TResult Get<TResult>(string key, TResult defaultValue = default);
        void SetFirstUsage();
    }
}
