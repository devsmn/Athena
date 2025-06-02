namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides common functionality for data repositories.
    /// </summary>
    public interface IAthenaRepository
    {
        /// <summary>
        /// Asynchronously initializes the repository.
        /// <para>
        /// This can be used to initialize required resources.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> InitializeAsync(IContext context);

        /// <summary>
        /// Registers patches to maintain compatibility. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compatService"></param>
        void RegisterPatches(IContext context, ICompatibilityService compatService);

        /// <summary>
        /// Executes registered patches (<see cref="RegisterPatches"/>).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compatService"></param>
        /// <returns></returns>
        Task ExecutePatches(IContext context, ICompatibilityService compatService);
    }
}
