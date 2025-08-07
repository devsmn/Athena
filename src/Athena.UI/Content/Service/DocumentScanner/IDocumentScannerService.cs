namespace Athena.UI
{
    public interface IDocumentScannerService
    {
        void Launch(Action<string[]> scannedImagesPaths, Action<Exception> onError, Action onCancelled);
        void ValidateInstallation(Action<bool> onChecked, Action<Exception> onError);
        bool FirstUsage();
        void SetFirstUsage();
    }
}
