namespace Athena.DataModel.Core
{
    public interface IAthenaRepository
    {
        public Task<bool> InitializeAsync(IContext context);
        void RegisterPatches(IContext context, ICompatibilityService compatService);
        Task ExecutePatches(IContext context, ICompatibilityService compatService);
    }
}
