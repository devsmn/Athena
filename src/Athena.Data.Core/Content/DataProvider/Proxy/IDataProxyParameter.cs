using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.Core
{
    /// <summary>
    /// <see cref="IDataProxyParameter"/> is the base interface for defining proxy parameters 
    /// that should be considered when retrieving data repositories.
    /// </summary>
    public interface IDataProxyParameter
    {
        Version MinimumVersion
        {
            get; set;
        }
    }
}
