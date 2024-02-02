using Athena.DataModel.Core;
using System.Diagnostics;

namespace Athena.DataModel
{
    public static class DataStore
    {
        private static readonly Dictionary<Type, IAthenaRepository> stores = new Dictionary<Type, IAthenaRepository>();

        public static void Register<TRepository>(TRepository repository)
            where TRepository : class, IAthenaRepository
        {
            try
            {
                stores.Add(repository.GetType(), repository);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public static async Task InitializeAsync()
        {
            foreach (var instance in stores.Values)
            {
                try
                {
                    // TODO: Parallel
                    await instance.InitializeAsync();
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
            foreach (var store in stores)
            {
                if (store.Value is TRepository value)
                    return value;
            }

            return default;
        }
    }
}
