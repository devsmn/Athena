using System.Text;
using Athena.DataModel.Core;

namespace Athena.UI
{
    internal interface IPdfCreatorService
    {
        /// <summary>
        /// Creates a PDF with the given parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="summary"></param>
        /// <param name="name"></param>
        /// <param name="images"></param>
        /// <param name="detectText"></param>
        /// <param name="pdfText"></param>
        /// <returns></returns>
        Task<byte[]> CreateAsync(
            IContext context,
            PdfCreationSummary summary,
            string name,
            IEnumerable<DocumentImageViewModel> images,
            bool detectText,
            StringBuilder pdfText);

        Action<string> Report { get; set; }
    }
}
