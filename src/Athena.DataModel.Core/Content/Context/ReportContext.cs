namespace Athena.DataModel.Core
{
    public class ReportContext : AthenaAppContext
    {
        private readonly Action<string> _report;

        public ReportContext(Action<string> report)
        {
            _report = report;
        }

        public override void Log(string message)
        {
            base.Log(message);
            _report.Invoke(message);
        }
    }
}
