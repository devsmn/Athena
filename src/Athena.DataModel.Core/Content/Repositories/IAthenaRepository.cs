namespace Athena.DataModel.Core
{
    public interface IAthenaRepository
    {
        public Task<bool> InitializeAsync();
    }
}
