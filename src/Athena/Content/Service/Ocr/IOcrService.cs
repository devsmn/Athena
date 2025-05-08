using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
