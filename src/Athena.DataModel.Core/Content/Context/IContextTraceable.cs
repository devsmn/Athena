namespace Athena.DataModel.Core
{
    public interface IContextTraceable
    {
        Guid CorrelationId
        {
            get;
        }

        int ThreadId
        {
            get;
        }
    }
}
