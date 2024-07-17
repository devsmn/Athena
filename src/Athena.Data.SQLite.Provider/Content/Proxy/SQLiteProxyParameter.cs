using Athena.Data.Core;

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
