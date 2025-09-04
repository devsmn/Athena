namespace Athena.UI
{
    using DataModel.Core;

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
        public IntegerEntityKey ParentReference { get; }

        /// <summary>
        /// Gets or sets whether the update was handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUpdate{TEntity}"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="parentReference"></param>
        public RequestUpdate(TEntity entity, UpdateType type, IntegerEntityKey parentReference)
        {
            Entity = entity;
            Type = type;
            ParentReference = parentReference;
            Handled = false;
        }

        public static implicit operator TEntity(RequestUpdate<TEntity> update)
        {
            return update.Entity;
        }
    }

}
