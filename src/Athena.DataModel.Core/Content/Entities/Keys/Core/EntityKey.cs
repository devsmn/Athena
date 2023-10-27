using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{

    public abstract class EntityKey
    {
        public EntityKey()
        {
        }
    }

    public abstract class EntityKey<TValue> : EntityKey
    {
        public TValue Id
        {
            get; set;
        }

        public EntityKey(TValue value)
            : base()
        {
            this.Id = value;
        }
    }
}
