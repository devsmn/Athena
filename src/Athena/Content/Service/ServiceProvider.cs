using System.Diagnostics;

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
