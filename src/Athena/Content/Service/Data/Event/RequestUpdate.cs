namespace Athena.UI
{
    using Athena.DataModel.Core;

    public class RequestUpdate<TEntity> where TEntity : Entity
    {
        private readonly TEntity _entity;
        private readonly UpdateType _updateType;
        private readonly EntityKey _parentReference;

        public TEntity Entity
        {
            get { return _entity; }
        }

        public UpdateType Type
        {
            get { return _updateType; }
        }

        public EntityKey ParentReference
        {
            get { return _parentReference; }
        }

        public RequestUpdate(TEntity entity, UpdateType type, EntityKey parentReference)
        {
            this._entity = entity;
            this._updateType = type;
            this._parentReference = parentReference;
        }

        public static implicit operator TEntity(RequestUpdate<TEntity> update)
        {
            return update.Entity;
        }
    }

}
