namespace Athena.DataModel.Core
{
    public class VersionPatch
    {
        public int Version { get; }

        private readonly List<Func<IContext, Task>> _patches;

        /// <summary>
        /// Initializes a new instance of <see cref="VersionPatch"/>.
        /// </summary>
        /// <param name="version">The version this patch is applied to</param>
        /// <param name="patches">The patches to be applied</param>
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

        /// <summary>
        /// Executes the patches.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task PatchAsync(IContext context)
        {
            foreach (var patch in _patches)
                await patch(context);
        }
    }
}
