using System.Diagnostics;
using Android.OS.Strictmode;
using AndroidX.Activity;
using Athena.DataModel.Core;
using Athena.Platforms.Android;
using Com.Spflaum.Documentscanner;
using Java.Interop;

namespace Athena.UI
{
    internal class DefaultDocumentScannerService : IDocumentScannerService
    {
        private static readonly Lazy<DefaultDocumentScannerService> InstanceFactory = new(() => new DefaultDocumentScannerService());
        public static DefaultDocumentScannerService Instance => InstanceFactory.Value;

        private Action<string[]> _onScanned;
        private Action<Exception> _onError;
        private readonly DocumentScannerWrapper _wrapper;

        public DefaultDocumentScannerService()
        {
            DocumentScannerCallback callback = new(
                exception =>
                {
                    _onError?.Invoke(exception);
                },
                imagePaths =>
                {
                    _onScanned?.Invoke(imagePaths);
                    _onScanned = null;
                });

            _wrapper = new DocumentScannerWrapper(callback);
        }

        public void Launch(Action<string[]> scannedImagesPaths, Action<Exception> onError)
        {
            _onScanned = scannedImagesPaths;
            _onError = onError;
            _wrapper.LaunchScanner();
        }

        public void ValidateInstallation(Action<bool> onChecked, Action<Exception> onError)
        {
            DocumentScannerAvailabilityCallback callback = new(onChecked, onError);
            _wrapper.IsScannerInstalled(callback);
        }

        public bool FirstUsage()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();
            return prefService.IsFirstScannerUsage();
        }

        public void SetFirstUsage()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();
            prefService.SetFirstScannerUsage();
        }

        public static void InitializeActivity(Android.App.Activity activity)
        {
            ComponentActivity compAct = activity.JavaCast<ComponentActivity>();
            Instance._wrapper.Initialize(compAct);
        }
    }
}
