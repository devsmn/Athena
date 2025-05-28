using System.Text;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using Microsoft.Maui.Graphics.Platform;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace Athena.UI
{
    internal class DefaultPdfCreatorService : IPdfCreatorService
    {
        public static string AppPrefix = "Athena: AI Document Manager";

        public async Task<byte[]> CreateAsync(
            IContext context,
            PdfCreationSummary summary,
            string name,
            IEnumerable<DocumentImageViewModel> images,
            bool detectText,
            StringBuilder pdfText)
        {
            //Create a new PDF document
            PdfDocument doc = new PdfDocument();
            doc.Compression = PdfCompressionLevel.Normal;
            doc.DocumentInformation.Author = AppPrefix;
            doc.DocumentInformation.CreationDate = DateTime.UtcNow;
            doc.DocumentInformation.Title = $"{AppPrefix} - {name}";
            doc.DocumentInformation.Subject = $"{AppPrefix} - {name}";
            doc.DocumentInformation.Producer = AppPrefix;
            doc.DocumentInformation.Keywords = $"{AppPrefix};PDF;AI";

            int docIdx = 0;

            foreach (var document in images)
            {
                summary.Add(document);

                try
                {
                    Report?.Invoke(string.Format(Localization.PdfCreationConvertingDocument, ++docIdx));

                    if (string.IsNullOrWhiteSpace(document.ImagePath))
                    {
                        continue;
                    }

                    if (document.IsPdf)
                    {
                        Report?.Invoke(string.Format(Localization.PdfCreationLoadingPdf, document.FileName));
                        FileStream pdfStream = new FileStream(document.ImagePath, FileMode.Open, FileAccess.Read);
                        PdfLoadedDocument loadedDoc = new PdfLoadedDocument(pdfStream);
                        loadedDoc.FileStructure.IncrementalUpdate = false;

                        Report?.Invoke(string.Format(Localization.PdfCreationMergingPdf, document.FileName));
                        PdfDocumentBase.Merge(doc, loadedDoc);

                        summary.Report(document.Id, Localization.PdfCreationPdfLoaded, ReportIssueLevel.Success);

                        if (detectText)
                        {
                            Report?.Invoke(string.Format(Localization.PdfCreationExtractingTextFromPdf, document.FileName));
                            foreach (PdfLoadedPage loadedPage in loadedDoc.Pages)
                            {
                                pdfText.AppendLine(loadedPage.ExtractText(true));
                            }

                            summary.Report(document.Id, Localization.PdfCreationTextDetected, ReportIssueLevel.Success);
                        }
                        else
                        {
                            summary.Report(document.Id, Localization.PdfCreationTextDetectionDisabled, ReportIssueLevel.Info);
                        }

                        continue;
                    }

                    FileStream imageStream = new FileStream(document.ImagePath, FileMode.Open, FileAccess.Read);

                    Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(imageStream).Downsize(1920, true);
                    MemoryStream ms = new MemoryStream();

                    await img.SaveAsync(ms, ImageFormat.Jpeg, 0.92f);

                    PdfBitmap image = new PdfBitmap(ms);
                    PdfUnitConverter converter = new PdfUnitConverter();
                    var size = converter.ConvertFromPixels(image.PhysicalDimension, PdfGraphicsUnit.Pixel);
                    PdfSection section = doc.Sections.Add();

                    section.PageSettings.Size = size;
                    section.PageSettings.Margins.All = 0;
                    var page = section.Pages.Add();

                    page.Graphics.DrawImage(image, 0, 0, size.Width, size.Height);
                    summary.Report(document.Id, Localization.PdfCreationImageCompressed, ReportIssueLevel.Success);
                }
                catch (Exception ex)
                {
                    summary.Report(document.Id, ex.Message, ReportIssueLevel.Error);
                }
            }

            //Creating the stream object
            MemoryStream preCompressStream = new MemoryStream();

            Report?.Invoke(Localization.PdfCreationSavingPdf);

            doc.Save(preCompressStream);

            //If the position is not set to '0' then the PDF will be empty
            preCompressStream.Position = 0;
            doc.Close(true);

            Report?.Invoke(Localization.PdfCreationSavingDocument);

            byte[] pdf = preCompressStream.ToArray();

            preCompressStream.Close();
            await preCompressStream.DisposeAsync();

            return pdf;
        }

        public Action<string> Report { get; set; }
    }
}
