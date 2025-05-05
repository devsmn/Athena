namespace Athena.DataModel.Core
{
    public class VersionPatch
    {
        public int Version { get; }

        private readonly List<Func<IContext, Task>> _patches;

        public VersionPatch(int version, params Func<IContext, Task>[] patches)
        {
            Version = version;
            _patches = new();
            _patches.AddRange(patches);
        }

        public void AddPatch(Func<IContext, Task> action)
        {
            _patches.Add(action);
        }

        public async Task PatchAsync(IContext context)
        {
            foreach (var patch in _patches)
                await patch(context);
        }

    }
    public interface ICompatibilityService
    {
        int GetLastUsedVersion();
        IEnumerable<VersionPatch> GetPatches<TFor>();
        void RegisterPatch<TFor>(VersionPatch patch);
        int GetCurrentVersion();
        void UpdateLastUsedVersion();
    }
}
