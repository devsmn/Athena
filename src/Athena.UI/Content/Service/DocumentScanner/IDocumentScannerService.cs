namespace Athena.UI
{
    public interface IDocumentScannerService
    {
        /// <summary>
        /// Launches the document scanner.
        /// </summary>
        /// <param name="scannedImagesPaths"></param>
        /// <param name="onError"></param>
        /// <param name="onCancelled"></param>
        void Launch(Action<string[]> scannedImagesPaths, Action<Exception> onError, Action onCancelled);

        /// <summary>
        /// Validates the installation of the document scanner.
        /// </summary>
        /// <param name="onChecked"></param>
        /// <param name="onError"></param>
        void ValidateInstallation(Action<bool> onChecked, Action<Exception> onError);

        /// <summary>
        /// Gets whether the document scanner is used for the first time.
        /// </summary>
        /// <returns></returns>
        bool FirstUsage();

        /// <summary>
        /// Sets that the document scanner was used for the first time.
        /// </summary>
        void SetFirstUsage();
    }
}
