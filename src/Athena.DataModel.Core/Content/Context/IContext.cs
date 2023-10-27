using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public interface IContext : IStateAware, IContextTraceable
    {
        void Log(string message);
        void Log(Exception exception);
        void Log(AggregateException aggregateException);
    }
}
