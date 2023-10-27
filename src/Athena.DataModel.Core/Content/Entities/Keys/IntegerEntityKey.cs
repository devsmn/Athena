using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public class IntegerEntityKey : EntityKey<int>
    {
        public static int TemporaryId { get; } = -1;

        public IntegerEntityKey(int key)
            : base(key)
        {
        }

        public bool TryParse(string id, out IntegerEntityKey key)
        {
            key = null;

            return false;
        }
    }
}
