using System.Diagnostics;

namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides access to registered services.
    /// <para>
    /// Remark: Do not pass the services via DI to the constructor. Currently, this take too many
    /// resources due to reflection.</para>
    /// </summary>
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
