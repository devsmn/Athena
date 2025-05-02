namespace Athena.DataModel.Core
{
    public class DefaultCompatibilityService : ICompatibilityService
    {
        private readonly Dictionary<Type, List<VersionPatch>> _patches;

        public DefaultCompatibilityService()
        {
            _patches = new();
        }

        public IEnumerable<VersionPatch> GetPatches<TFor>()
        {
            if (!_patches.TryGetValue(typeof(TFor), out var list))
                yield break;

            int fromVersion = GetLastUsedVersion();

            foreach (var patch in list)
            {
                if (fromVersion < patch.Version)
                    yield return patch;
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
            var prefService = Services.GetService<IPreferencesService>();
            return prefService.GetLastUsedVersion();
        }

        public int GetCurrentVersion()
        {
            if (!Int32.TryParse(AppInfo.Current.BuildString, out int version))
                version = 55;

            return version;
        }

        public void UpdateLastUsedVersion()
        {
            Services.GetService<IPreferencesService>().SetLastUsedVersion(GetCurrentVersion());
        }
    }
}
