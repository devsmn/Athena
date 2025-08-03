#if ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.Spflaum.Documentscanner;
using Exception = Java.Lang.Exception;

namespace Athena.Platforms.Android
{
    internal class DocumentScannerCallback : Java.Lang.Object, Com.Spflaum.Documentscanner.IScanCallback
    {
        private readonly Action<System.Exception> _errorCallback;
        private readonly Action<string[]> _scannedCallback;

        public DocumentScannerCallback(Action<System.Exception> onError, Action<string[]> onScanned)
        {
            _errorCallback = onError;
            _scannedCallback = onScanned;
        }

        public void OnError(Exception p0)
        {
            _errorCallback(new System.Exception(p0.ToString()));
        }

        public void OnScanned(string[] p0)
        {
            _scannedCallback(p0);
        }
    }
}
#endif
