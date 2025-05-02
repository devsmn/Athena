using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public enum ReportIssueLevel
    {
        None,
        Info,
        Warning,
        Error,
        Success
    }

    public partial class PdfCreationSummaryStep : ObservableObject
    {
        public int LevelInt => (int)Level;

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private ObservableCollection<PdfCreationSummaryStep> _items;

        public ReportIssueLevel Level { get; set; }


        public PdfCreationSummaryStep(string fileName, ReportIssueLevel level)
        {
            Message = fileName;
            Level = level;
            Items = new ObservableCollection<PdfCreationSummaryStep>();
        }

        public void Report(string message, ReportIssueLevel level)
        {
            Items.Add(new PdfCreationSummaryStep(message, level));
        }
    }

}
