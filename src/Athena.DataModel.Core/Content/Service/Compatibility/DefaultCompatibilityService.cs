namespace Athena.DataModel.Core
{
    /// <summary>
    /// The default implementation of the <see cref="ICompatibilityService"/>.
    /// </summary>
    public class DefaultCompatibilityService : ICompatibilityService
    {
        private readonly Dictionary<Type, List<VersionPatch>> _patches;

        public DefaultCompatibilityService()
        {
            _patches = new();
        }

        public IEnumerable<VersionPatch> GetPatches<TFor>(IContext context)
        {
            if (!_patches.TryGetValue(typeof(TFor), out List<VersionPatch> list))
            {
                context.Log($"No patches available for type=[{typeof(TFor).FullName}]");
                yield break;
            }

            int fromVersion = GetLastUsedVersion();

            foreach (VersionPatch patch in list)
            {
                if (fromVersion < patch.Version)
                {
                    context.Log($"Patch with version=[{patch.Version}] found, fromVersion=[{fromVersion}] for type=[{typeof(TFor).FullName}]");
                    yield return patch;
                }
            }
        }

        public void RegisterPatch<TFor>(VersionPatch patch)
        {
            if (!_patches.ContainsKey(typeof(TFor)))
                _patches.Add(typeof(TFor), new List<VersionPatch>());

            _patches[typeof(TFor)].Add(patch);
        }

        public int GetLastUsedVersion()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();
            return prefService.GetLastUsedVersion();
        }

        public int GetCurrentVersion()
        {
            if (!Int32.TryParse(AppInfo.Current.BuildString, out int version))
                version = 55;

            return version;
        }

        public void UpdateLastUsedVersion(IContext context)
        {
            int version = GetCurrentVersion();
            context.Log($"Setting last used version to [{version}]");

            Services.GetService<IPreferencesService>().SetLastUsedVersion(version);
        }
    }
}
