using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.UI;

namespace Athena.UI
{
    internal interface IPdfCreatorService
    {
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
