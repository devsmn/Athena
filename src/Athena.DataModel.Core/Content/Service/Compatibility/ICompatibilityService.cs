namespace Athena.DataModel.Core
{
    /// <summary>
    /// Provides functionality to keep compatibility between one or more versions. 
    /// </summary>
    public interface ICompatibilityService
    {
        /// <summary>
        /// Gets a list of <see cref="VersionPatch"/> for the given entity <typeparamref name="TFor"/>.
        /// </summary>
        /// <typeparam name="TFor"></typeparam>
        /// <returns></returns>
        IEnumerable<VersionPatch> GetPatches<TFor>(IContext context);

        /// <summary>
        /// Registers a <see cref="VersionPatch"/> for the given entity <typeparamref name="TFor"/>.
        /// </summary>
        /// <typeparam name="TFor"></typeparam>
        /// <param name="patch"></param>
        void RegisterPatch<TFor>(VersionPatch patch);

        /// <summary>
        /// Updates the last used version.
        /// </summary>
        void UpdateLastUsedVersion(IContext context);

        /// <summary>
        /// Gets the current app version.
        /// </summary>
        /// <returns></returns>
        int GetCurrentVersion();
    }
}
