using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public static class ServiceProvider
    {
        private static IServiceProvider serviceProvider;

        public static void Register(IServiceProvider serviceProvider)
        {
            ServiceProvider.serviceProvider = serviceProvider;
        }

        [DebuggerStepThrough]
        public static TService GetService<TService>()
        {
            if (serviceProvider == null)
            {
                throw new InvalidOperationException();
            }

            return serviceProvider.GetService<TService>();
        }
    }
}
