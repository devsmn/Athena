using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;
using TesseractOcrMaui.Results;

namespace Athena.UI
{
    public enum OcrError
    {
        None = 0,
        TrainedDataDirectoryMissing,
        TrainedDataDirectoryEmpty
    }

    public interface IOcrService
    {
        Task<RecognizionResult> RecognizeTextAsync(string path);
        OcrError Error { get; }
        void Reset();
        string[] GetInstalledLanguages(IContext context);
        void DeleteInstalledLanguages(IContext context);
        void DeleteInstalledLanguages(IContext context, IEnumerable<string> languages);
        Task DownloadLanguagesAsync(IContext context, IEnumerable<string> languages);
    }
}
