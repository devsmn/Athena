using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public interface INetworkService
    {
        bool IsWifi();
        bool IsInternetAccessPossible();
    }
}
