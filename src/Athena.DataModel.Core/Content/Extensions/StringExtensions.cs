using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public static class StringExtensions
    {
        public static string EmptyIfNull(this string value)
        {
            return value ?? string.Empty;
        }
    }
}
