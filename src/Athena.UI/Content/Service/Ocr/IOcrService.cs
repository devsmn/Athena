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
        /// <summary>
        /// Asynchronously recognizes text in the image at the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<RecognizionResult> RecognizeTextAsync(string path);

        /// <summary>
        /// Gets the current <see cref="OcrError"/>.
        /// </summary>
        OcrError Error { get; }

        /// <summary>
        /// Resets the ocr service.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets the installed ocr languages.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string[] GetInstalledLanguages(IContext context);

        /// <summary>
        /// Deletes all installed ocr languages.
        /// </summary>
        /// <param name="context"></param>
        void DeleteInstalledLanguages(IContext context);

        /// <summary>
        /// Deletes the ocr languages specified in <paramref name="languages"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="languages"></param>
        void DeleteInstalledLanguages(IContext context, IEnumerable<string> languages);

        /// <summary>
        /// Asynchronously downloads the given <paramref name="languages"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="languages"></param>
        /// <returns></returns>
        Task DownloadLanguagesAsync(IContext context, IEnumerable<string> languages);
    }
}
