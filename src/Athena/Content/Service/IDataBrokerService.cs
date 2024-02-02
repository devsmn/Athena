namespace Athena.UI
{
    using Athena.DataModel.Core;

    internal interface IDataBrokerService
    {
        void Publish<TEntity>(IContext context, TEntity entity, UpdateType type) where TEntity : Entity;
        void Publish<TEntity>(IContext context, TEntity entity, UpdateType type, EntityKey parentReference) where TEntity : Entity;
        void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type) where TEntity : Entity;
        void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type, EntityKey parentReference) where TEntity : Entity;

        void PrepareForLoading();
        void RaiseAppInitialized();

        TEntity Request<TEntity>(IContext context, EntityKey key) where TEntity : Entity;
        IEnumerable<TEntity> Request<TEntity>(IContext context);

        event EventHandler PublishStarted;
        event EventHandler<DataPublishedEventArgs> Published;
        event EventHandler AppInitialized;
    }
}
