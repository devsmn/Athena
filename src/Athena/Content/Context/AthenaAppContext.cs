using System.Diagnostics;
using System.Text;
using Athena.DataModel.Core;

namespace Athena.UI
{
    public class AthenaAppContext : AthenaContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AthenaAppContext"/>.
        /// </summary>
        [DebuggerStepThrough]
        public AthenaAppContext()
        {
            CancellationToken = CancellationToken.None;
            CorrelationId = Guid.NewGuid();
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string ToString()
        {
            return $"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}, CancellationToken=[{CancellationToken}]";
        }

        public override void Log(string message)
        {
            // TODO log to play console
            string logMsg = $"{GetLogPrefix(false)}{message}";
            Debug.WriteLine(logMsg);
        }

        public override void Log(Exception exception)
        {
            string logMsg = $"{GetLogPrefix(true)}{exception}";
            Debug.WriteLine(logMsg);
        }

        public override void Log(AggregateException aggregateException)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetLogPrefix(true) + aggregateException.Message);

            foreach (var ex in aggregateException.InnerExceptions)
            {
                sb.Append(ex);
            }

            Debug.WriteLine(sb);
        }
    }
}
