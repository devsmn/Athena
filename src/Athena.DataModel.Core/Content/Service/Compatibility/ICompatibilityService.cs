using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kotlin;

namespace Athena.DataModel.Core
{
    public class VersionPatch
    {
        public int Version { get; }

        private readonly List<Func<Task>> _patches;

        public VersionPatch(int version, params Func<Task>[] patches)
        {
            Version = version;
            _patches = new();
            _patches.AddRange(patches);
        }

        public void AddPatch(Func<Task> action)
        {
            _patches.Add(action);
        }

        public async Task PatchAsync()
        {
            foreach (var patch in _patches)
                await patch();
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
