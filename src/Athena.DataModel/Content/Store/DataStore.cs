using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public static class DataStore
    {
        private static readonly Dictionary<Type, IAthenaRepository> _stores = new();

        public static void Register<TRepository>(TRepository repository)
            where TRepository : class, IAthenaRepository
        {
            try
            {
                _stores.Add(repository.GetType(), repository);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public static async Task InitializeAsync(IContext context)
        {
            ICompatibilityService compatService = Services.GetService<ICompatibilityService>();

            foreach (var instance in _stores.Values)
            {
                try
                {
                    // TODO: Parallel?
                    await instance.InitializeAsync(context);
                    instance.RegisterPatches(context, compatService);
                    await instance.ExecutePatches(context, compatService);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Debug.Assert(false);
                }
            }
        }

        [DebuggerStepThrough]
        internal static TRepository Resolve<TRepository>()
            where TRepository : IAthenaRepository
        {
            foreach (var store in _stores)
            {
                if (store.Value is TRepository value)
                    return value;
            }

            return default;
        }
    }
}
