using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.UI;

namespace Athena.UI
{
    internal class PdfCreationSummary
    {
        private readonly Dictionary<Guid, PdfCreationSummaryStep> _reports;

        public IEnumerable<PdfCreationSummaryStep> Reports
        {
            get { return _reports.Select(report => report.Value); }
        }

        public PdfCreationSummary()
        {
            _reports = new();
        }

        public void Add(DocumentImageViewModel document)
        {
            string fileName = document.FileName;

            if (string.IsNullOrEmpty(fileName))
                fileName = $"Document #{_reports.Count + 1}";

            _reports.Add(document.Id, new PdfCreationSummaryStep(fileName, ReportIssueLevel.None));
        }

        public void Report(Guid id, string message, ReportIssueLevel level)
        {
            if (!_reports.TryGetValue(id, out var report))
                return;

            report.Report(message, level);
        }

        public void Finish()
        {
            foreach (var report in _reports.Values)
            {
                report.Level = report.Items.Any(x => x.Level == ReportIssueLevel.Error || x.Level == ReportIssueLevel.Warning)
                    ? ReportIssueLevel.Warning
                    : ReportIssueLevel.Success;
            }
        }
    }


}
