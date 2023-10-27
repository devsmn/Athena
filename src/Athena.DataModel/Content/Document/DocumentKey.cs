using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public class DocumentKey : IntegerEntityKey
    {
        public DocumentKey(int key) : base(key)
        {
        }
    }
}
