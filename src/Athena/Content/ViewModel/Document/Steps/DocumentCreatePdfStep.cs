using System.Collections.ObjectModel;
using System.Text;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using TesseractOcrMaui.Enums;
using TesseractOcrMaui.Results;

namespace Athena.UI
{
    /// <summary>
    /// Implements the PDF creation step when adding a new document.
    /// </summary>
    internal class DocumentCreatePdfStep : IViewStep<DocumentEditorViewModel>
    {
        private DocumentEditorViewModel _vm;
        private IContext _context;

        public async Task ExecuteAsync(IContext context, DocumentEditorViewModel vm)
        {
            _vm = vm;
            _context = context;
            await CreatePdfStep();
        }

        private async Task CreatePdfStep()
        {
            _vm.IsBusy = true;
            Report("Converting document to pdf...");

            PdfCreationSummary summary = new();
            StringBuilder pdfText = new();

            _vm.ShowAd();
            _vm.Document.Pdf = await CreatePdf(_context, summary, pdfText);
            await PostProcessPdf(_context, summary, pdfText);
            summary.Finish();

            Report("Publishing changes...");

            IDataBrokerService dataService = Services.GetService<IDataBrokerService>();

            dataService.Publish(_context, _vm.Document.Document, UpdateType.Add, _vm.ParentFolder.Key);
            dataService.Publish(_context, _vm.Document.Document, UpdateType.Edit, _vm.ParentFolder.Key);

            _vm.IsBusy = false;
            Report("Finished!");

            _vm.DocumentReports = new ObservableCollection<PdfCreationSummaryStep>(summary.Reports);
        }

        private void Report(string msg)
        {
            MainThread.BeginInvokeOnMainThread(() => _vm.BusyText = msg);
        }

        private async Task<byte[]> CreatePdf(IContext context, PdfCreationSummary summary, StringBuilder pdfText)
        {
            IPdfCreatorService pdfCreator = Services.GetService<IPdfCreatorService>();
            pdfCreator.Report = Report;

            return await Task.Run(async () => await pdfCreator.CreateAsync(
                context,
                summary,
                _vm.Document.Name,
                _vm.Images,
                _vm.DetectText,
                pdfText));
        }

        private async Task PostProcessPdf(IContext context, PdfCreationSummary summary, StringBuilder pdfText)
        {
            Report("Saving tags...");

            foreach (var tag in _vm.SelectedTags)
            {
                _vm.Document.AddTag(context, tag);
            }

            _vm.ParentFolder.AddDocument(_vm.Document);
            _vm.ParentFolder.Save(context, FolderSaveOptions.Documents);

            if (_vm.DetectText)
            {
                var ocrService = Services.GetService<IOcrService>();
                int docIdx = 0;

                StringBuilder sb = new StringBuilder();

                foreach (var document in _vm.Images)
                {
                    // Text in PDFs is already detected when loading the PDF. No need for OCR.
                    if (document.IsPdf)
                        continue;

                    Report(string.Format(Localization.PdfCreationDetectingTextInDocument, ++docIdx));

                    var result = await ocrService.RecognizeTextAsync(document.ImagePath);

                    if (!result.FinishedWithSuccess())
                    {
                        if (result.Status == RecognizionStatus.CannotLoadTessData)
                        {
                            if (ocrService.Error != OcrError.TrainedDataDirectoryMissing)
                            {
                                summary.Report(
                                    document.Id,
                                    Localization.PdfCreationNoLanguagesConfigured,
                                    ReportIssueLevel.Error);
                            }
                        }

                        summary.Report(
                            document.Id,
                            string.Format(Localization.PdfCreationTextDetectionFailed, result.Message),
                            ReportIssueLevel.Error);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(result.RecognisedText))
                        {
                            summary.Report(
                                document.Id,
                                Localization.PdfCreationDocumentContainsNoText,
                                ReportIssueLevel.Warning);
                        }
                        else
                        {
                            sb.Append(result.RecognisedText);
                            sb.AppendLine();

                            summary.Report(
                                document.Id,
                                Localization.PdfCreationDetectedText,
                                ReportIssueLevel.Success);

                            if (result.Confidence < 0.7)
                            {
                                summary.Report(
                                    document.Id,
                                    Localization.PdfCreationBadImageQuality,
                                    ReportIssueLevel.Warning);
                            }
                        }
                    }
                }

                sb.Append(pdfText);

                if (sb.Length > 0)
                {
                    Chapter chapter = new Chapter(_vm.Document.Key.Id, docIdx, sb.ToString())
                    {
                        FolderId = _vm.ParentFolder.Id.ToString()
                    };

                    chapter.Save(context);
                }
            }
        }
    }
}
