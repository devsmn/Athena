namespace Athena.UI
{
    internal class PdfCreationSummary
    {
        private readonly Dictionary<Guid, PdfCreationSummaryStep> _reports;

        /// <summary>
        /// Gets the available reports.
        /// </summary>
        public IEnumerable<PdfCreationSummaryStep> Reports
        {
            get { return _reports.Select(report => report.Value); }
        }

        public PdfCreationSummary()
        {
            _reports = new();
        }

        /// <summary>
        /// Adds the given <paramref name="document"/> to the reports.
        /// </summary>
        /// <param name="document"></param>
        public void Add(DocumentImageViewModel document)
        {
            string fileName = document.FileName;

            if (string.IsNullOrEmpty(fileName))
                fileName = $"Document #{_reports.Count + 1}";

            _reports.Add(document.Id, new PdfCreationSummaryStep(fileName, ReportIssueLevel.None));
        }

        /// <summary>
        /// Reports the given message.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public void Report(Guid id, string message, ReportIssueLevel level)
        {
            if (!_reports.TryGetValue(id, out PdfCreationSummaryStep report))
                return;

            report.Report(message, level);
        }

        /// <summary>
        /// Finishes the report.
        /// </summary>
        public void Finish()
        {
            foreach (PdfCreationSummaryStep report in _reports.Values)
            {
                report.Level = report.Items.Any(x => x.Level == ReportIssueLevel.Error || x.Level == ReportIssueLevel.Warning)
                    ? ReportIssueLevel.Warning
                    : ReportIssueLevel.Success;
            }
        }
    }
}
