using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{

    public class ReportContext : AthenaContext
    {
        private readonly Action<string> _report;

        public ReportContext(Action<string> report)
        {
            _report = report;
        }

        public override void Log(string message)
        {
            _report.Invoke(message);
        }

        public override void Log(Exception exception)
        {
        }

        public override void Log(AggregateException aggregateException)
        {
        }
    }
}
