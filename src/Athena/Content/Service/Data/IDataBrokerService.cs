using Athena.DataModel;

namespace Athena.UI
{
    using DataModel.Core;

    internal interface IDataBrokerService
    {
        /// <summary>
        /// Publishes an update for the given <paramref name="entity"/> with the given <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        void Publish<TEntity>(IContext context, TEntity entity, UpdateType type) where TEntity : Entity;

        /// <summary>
        /// Publishes an update for the given <paramref name="entity"/> with the given <paramref name="type"/>
        /// and the reference to the parent <paramref name="parentReference"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="parentReference"></param>
        void Publish<TEntity>(
                IContext context,
                TEntity entity,
                UpdateType type,
                IntegerEntityKey parentReference)
            where TEntity : Entity;

        /// <summary>
        /// Publishes an update for the given <paramref name="entities"/> with the given <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="type"></param>
        void Publish<TEntity>(
                IContext context,
                IEnumerable<TEntity> entities,
                UpdateType type)
            where TEntity : Entity;

        /// <summary>
        /// Publishes an update for the given <paramref name="entities"/> with the given <paramref name="type"/>
        /// and the reference to the parent <paramref name="parentReference"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="type"></param>
        /// <param name="parentReference"></param>
        void Publish<TEntity>(
                IContext context,
                IEnumerable<TEntity> entities,
                UpdateType type,
                IntegerEntityKey parentReference)
            where TEntity : Entity;

        /// <summary>
        /// Prepares the system for loading.
        /// </summary>
        void PrepareForLoading();

        /// <summary>
        /// Broadcasts that the app is initialized.
        /// </summary>
        void RaiseAppInitialized();

        /// <summary>
        /// Gets the root folder.
        /// </summary>
        /// <returns></returns>
        FolderViewModel GetRootFolder();

        /// <summary>
        /// Sets the root folder.
        /// </summary>
        /// <param name="folder"></param>
        void SetRootFolder(Folder folder);

        event EventHandler PublishStarted;
        event EventHandler<DataPublishedEventArgs> Published;
        event EventHandler AppInitialized;
    }
}
