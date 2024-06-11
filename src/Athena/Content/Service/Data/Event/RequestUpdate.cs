namespace Athena.UI
{
    using Athena.DataModel.Core;

    public class RequestUpdate<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets the related entity.
        /// </summary>
        public TEntity Entity { get; }

        /// <summary>
        /// Gets the <see cref="UpdateType"/>.
        /// </summary>
        public UpdateType Type { get; }

        /// <summary>
        /// Gets the optional reference to the parent entity.
        /// </summary>
        public EntityKey ParentReference { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUpdate{TEntity}"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="parentReference"></param>
        public RequestUpdate(TEntity entity, UpdateType type, EntityKey parentReference)
        {
            Entity = entity;
            Type = type;
            ParentReference = parentReference;
        }

        public static implicit operator TEntity(RequestUpdate<TEntity> update)
        {
            return update.Entity;
        }
    }

}
