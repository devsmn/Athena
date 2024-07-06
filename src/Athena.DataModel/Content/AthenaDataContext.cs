using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    internal class AthenaDataContext : AthenaContext
    {
        public AthenaDataContext()
        {
            CorrelationId = Guid.NewGuid();
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public override void Log(Exception exception)
        {
            Debug.WriteLine(exception);
        }

        public override void Log(AggregateException aggregateException)
        {
            foreach (var exception in aggregateException.InnerExceptions)
            {
                Log(exception);
            }
        }
    }
}
