using System.Diagnostics;
using System.Text;
using Serilog;

namespace Athena.DataModel.Core
{
    public class AthenaAppContext : AthenaContext
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="AthenaAppContext"/>.
        /// </summary>
        [DebuggerStepThrough]
        public AthenaAppContext()
        {
            CancellationToken = CancellationToken.None;
            CorrelationId = Guid.NewGuid();
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            _logger = Services.GetService<ILogger>();
        }

        public override string ToString()
        {
            return $"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}, CancellationToken=[{CancellationToken}]";
        }

        public override void Log(string message)
        {
            _logger.Information($"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}] -> {message}");
        }

        public override void Log(Exception exception)
        {
            _logger.Error($"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}] -> {exception}");
        }

        public override void Log(AggregateException aggregateException)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(aggregateException.Message);

            foreach (Exception ex in aggregateException.InnerExceptions)
            {
                sb.Append(ex);
            }

            _logger.Error($"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}] -> {sb}");
        }
    }
}
