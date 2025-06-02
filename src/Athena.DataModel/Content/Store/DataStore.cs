using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    /// <summary>
    /// Provides static access to data stores.
    /// </summary>
    public static class DataStore
    {
        private static readonly Dictionary<Type, IAthenaRepository> _stores = new();

        /// <summary>
        /// Registers the given <paramref name="repository"/>.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <param name="repository"></param>
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

        /// <summary>
        /// Clears the data stores.
        /// </summary>
        public static void Clear()
        {
            _stores.Clear();
        }

        /// <summary>
        /// Initializes the registered repositories.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves the instance for the given repository.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
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
