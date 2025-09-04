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
    internal class DocumentScannerAvailabilityCallback : Java.Lang.Object, Com.Spflaum.Documentscanner.IAvailabilityCallback
    {
        private readonly Action<bool> _onChecked;
        private readonly Action<System.Exception> _onError;

        public DocumentScannerAvailabilityCallback(Action<bool> onChecked, Action<System.Exception> onError)
        {
            _onChecked = onChecked;
            _onError = onError;
        }

        public void OnChecked(bool p0)
        {
            _onChecked(p0);
        }

        public void OnError(Exception p0)
        {
            _onError(new System.Exception(p0.ToString()));
        }
    }
}
#endif
