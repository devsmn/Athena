using Athena.DataModel.Core;

namespace Athena.Data.Core
{
    public interface IDataProviderPatcher
    {
        void RegisterPatches(ICompatibilityService service);
        Task ExecutePatchesAsync(IContext context, ICompatibilityService service);
    }
}
