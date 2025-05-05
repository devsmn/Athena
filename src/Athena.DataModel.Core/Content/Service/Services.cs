using System.Diagnostics;

namespace Athena.DataModel.Core
{
    public static class Services
    {
        private static IServiceProvider serviceProvider;

        public static void Register(IServiceProvider serviceProvider)
        {
            Services.serviceProvider = serviceProvider;
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
