using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public interface INetworkService
    {
        /// <summary>
        /// Determines whether the device is connected to a WLAN network.
        /// </summary>
        /// <returns></returns>
        bool IsWifi();

        /// <summary>
        /// Determines whether internet access is possible.
        /// </summary>
        /// <returns></returns>
        bool IsInternetAccessPossible();
    }
}
