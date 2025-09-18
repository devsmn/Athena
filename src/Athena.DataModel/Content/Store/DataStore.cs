using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    /// <summary>
    /// Provides static access to data stores.
    /// </summary>
    public static class DataStore
    {
        private static readonly Dictionary<Type, IAthenaRepository> Stores = new();

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
                Stores.Add(repository.GetType(), repository);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Closes all data stores.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task CloseAllAsync(IContext context)
        {
            if (Stores == null || Stores.Count == 0)
                return;

            foreach (IAthenaRepository repository in Stores.Values)
            {
                try
                {
                    await repository.CloseAsync();
                }
                catch (Exception ex)
                {
                    context.Log("Unable to close repository: ");
                    context.Log(ex);
                }
            }
        }

        /// <summary>
        /// Clears the data stores.
        /// </summary>
        public static void Clear()
        {
            Stores.Clear();
        }

        /// <summary>
        /// Initializes the registered repositories.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> InitializeAsync(IContext context, Action onCipherInvalid)
        {
            ICompatibilityService compatService = Services.GetService<ICompatibilityService>();

            foreach (IAthenaRepository instance in Stores.Values)
            {
                try
                {
                    // TODO: Parallel?
                    await instance.InitializeAsync(context);
                    instance.RegisterPatches(context, compatService);
                    await instance.ExecutePatches(context, compatService);
                }
                catch (InvalidCipherException)
                {
                    onCipherInvalid();
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }

            return true;
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
            foreach (KeyValuePair<Type, IAthenaRepository> store in Stores)
            {
                if (store.Value is TRepository value)
                    return value;
            }

            return default;
        }
    }
}
