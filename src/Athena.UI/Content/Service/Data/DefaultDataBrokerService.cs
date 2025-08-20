using System.Diagnostics;
using Athena.DataModel;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.Messaging;

namespace Athena.UI
{
    public class DefaultDataBrokerService : IDataBrokerService
    {
        private FolderViewModel _rootFolder;

        /// <inheritdoc />  
        public FolderViewModel GetRootFolder()
        {
            return _rootFolder;
        }

        /// <inheritdoc />  
        public void SetRootFolder(Folder folder)
        {
            _rootFolder = new FolderViewModel(folder);
        }

        /// <inheritdoc />  
        public void PrepareForLoading()
        {
            RaisePublishStarted();
        }

        private void RaisePublishStarted()
        {
            WeakReferenceMessenger.Default.Send<DataPublishStartedMessage>();
        }

        /// <inheritdoc />  
        public void RaiseAppInitialized()
        {
            WeakReferenceMessenger.Default.Send<AppInitializedMessage>();
        }

        /// <inheritdoc />  
        public void Publish<TEntity>(IContext context, TEntity entity, UpdateType type, IntegerEntityKey parentReference)
            where TEntity : Entity
        {
            Publish(context, new List<TEntity> { entity }, type, parentReference);
        }

        /// <inheritdoc />  
        public void Publish<TEntity>(IContext context, TEntity entity, UpdateType type)
            where TEntity : Entity
        {
            Publish(context, entity, type, null);
        }

        /// <inheritdoc />  
        public void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type) where TEntity : Entity
        {
            Publish(context, entities, type, null);
        }

        /// <inheritdoc />  
        public void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type, IntegerEntityKey parentReference) where TEntity : Entity
        {
            TaskScheduler syncContext = TaskScheduler.Current;

            Task.Run(() =>
            {
                RaisePublishStarted();

                List<RequestUpdate<TEntity>> updates = entities
                    .Select(entity => new RequestUpdate<TEntity>(entity, type, parentReference))
                    .ToList();

                RaisePublished(updates);
            }).ContinueWith(
                task =>
                {
                    if (task.Exception == null)
                        return;

                    context.Log(task.Exception);
                    throw task.Exception;
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                syncContext);
        }

        private void RaisePublished<TEntity>(IEnumerable<RequestUpdate<TEntity>> updates)
            where TEntity : Entity
        {
            DataPublishedArgs args = new DataPublishedArgs();

            foreach (RequestUpdate<TEntity> update in updates)
            {
                switch (update.Entity)
                {
                    case Document:
                        args.Documents.Add(update as RequestUpdate<Document>);
                        break;

                    case Folder:
                        args.Folders.Add(update as RequestUpdate<Folder>);
                        break;

                    case Tag:
                        args.Tags.Add(update as RequestUpdate<Tag>);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            WeakReferenceMessenger.Default.Send(args);
        }
    }
}

