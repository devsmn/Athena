namespace Athena.DataModel.Core
{
    public interface IStateAware
    {
        CancellationToken CancellationToken
        {
            get; 
        }
    }
}
