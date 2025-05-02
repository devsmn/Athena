namespace Athena.DataModel.Core
{
    public interface IAthenaRepository
    {
        public Task<bool> InitializeAsync();
        void RegisterPatches(ICompatibilityService compatService);
        Task ExecutePatches(ICompatibilityService compatService);
    }
}
