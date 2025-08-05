namespace Athena.DataModel.Core
{
    public abstract class AthenaContext : IContext
    {
        public CancellationToken CancellationToken
        {
            get;
            set;
        }

        public Guid CorrelationId
        {
            get;
            set;
        }

        public int ThreadId
        {
            get;
            set;
        }


        public abstract void Log(string message);

        public abstract void Log(Exception exception);

        public abstract void Log(AggregateException aggregateException);
    }
}
