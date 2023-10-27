using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Athena.DataModel.Core
{
    public class AthenaContext : IContext
    {
        public CancellationToken CancellationToken
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Guid CorrelationId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int ThreadId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Log(string message)
        {
            StringBuilder sb = new StringBuilder();

            string logMsg = $"[{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} - #{this.ThreadId}] {{{this.CorrelationId}}}: {message}";

            Debug.WriteLine(logMsg);
        }

        public void Log(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Log(AggregateException aggregateException)
        {
            throw new NotImplementedException();
        }
    }
}
