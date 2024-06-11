using Athena.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.SQLite.Proxy
{
    public class SqLiteProxyParameter : IDataProxyParameter
    {
        public Version MinimumVersion
        {
            get; set;
        }
    }
}
