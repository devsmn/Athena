namespace Athena.DataModel.Core
{
    public class Entity
    {
        public DateTime CreationDate
        {
            get; set;
        }

        public DateTime ModDate
        {
            get; set;
        }

        public Entity()
        {
            CreationDate = DateTime.UtcNow;
        }

    }

    public class Entity<TKey> : Entity
        where TKey : class
    {
       
        public TKey Key
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Entity{TEntityKey}"/>.
        /// </summary>
        /// <param name="key"></param>
        public Entity(TKey key)
        {
            Key = key;
            CreationDate = DateTime.UtcNow;
            ModDate = DateTime.UtcNow;
        }

        public virtual void SetKey(TKey key)
        {
            this.Key = key;
        }

    }
}
