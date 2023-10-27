using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public interface IAthenaRepository
    {
        public abstract Task<bool> InitializeAsync();
    }
}
