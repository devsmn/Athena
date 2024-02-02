using Athena.DataModel.Core;
using System.Diagnostics;
using System.Text;

namespace Athena.UI
{
    public class AthenaAppContext : IContext
    {
        public CancellationToken CancellationToken
        {
            get;
        }

        public Guid CorrelationId
        {
            get; 
        }

        public int ThreadId
        {
            get;
        }

        public AthenaAppContext()
        {
            CancellationToken = new CancellationToken();
            CorrelationId = Guid.NewGuid();
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string ToString()
        {
            return $"CorrelationId=[{CorrelationId}], ThreadId=[{ThreadId}, CancellationToken=[{CancellationToken}]";
        }
        
        public void Log(string message)
        {
            string logMsg = $"{GetLogPrefix(false)}{message}";
            Debug.WriteLine(logMsg);
        }

        public void Log(Exception exception)
        {
            string logMsg = $"{GetLogPrefix(true)}{exception}";
            Debug.WriteLine(logMsg);
        }

        public void Log(AggregateException aggregateException)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append(GetLogPrefix(true) + aggregateException.Message);

            foreach (var ex in aggregateException.InnerExceptions)
            {
                sb.Append(ex);
            }

            Debug.WriteLine(sb);
        }

        private string GetLogPrefix(bool isEx)
        {
            return $"{(isEx ? "-E-" : "-I-")} [{DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} - #{this.ThreadId}] {{{this.CorrelationId}}}: ";
        }
    }
}
