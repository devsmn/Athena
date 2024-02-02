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
