using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public class Entity
    {
        // ---- public properties ----
        public DateTime CreationDate
        {
            get; set;
        }

        public DateTime ModDate
        {
            get; set;
        }

    }

    public class Entity<TKey> : Entity
        where TKey : class
    {
       
        public TKey Key
        {
            get; private set;
        }

        // ---- constructor ----

        /// <summary>
        /// Initializes a new instance of <see cref="Entity{TEntityKey}"/>.
        /// </summary>
        /// <param name="key"></param>
        public Entity(TKey key)
        {
            this.Key = key;
            //this.CreationDate = DateTime.Now;
        }

        // ---- methods ----
        public virtual void SetKey(TKey key)
        {
            this.Key = key;
        }

    }
}
