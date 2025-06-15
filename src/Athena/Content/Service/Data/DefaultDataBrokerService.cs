using System.Diagnostics;
using Athena.DataModel;
using Athena.DataModel.Core;

namespace Athena.UI
{
    public class DefaultDataBrokerService : IDataBrokerService
    {
        public event EventHandler<DataPublishedEventArgs> Published;
        public event EventHandler AppInitialized;
        public event EventHandler PublishStarted;

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

        /// <inheritdoc />  
        public void RaiseAppInitialized()
        {
            AppInitialized?.Invoke(this, EventArgs.Empty);
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

        private void RaisePublishStarted()
        {
            PublishStarted?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePublished<TEntity>(IEnumerable<RequestUpdate<TEntity>> updates)
            where TEntity : Entity
        {
            DataPublishedEventArgs args = new DataPublishedEventArgs();

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

            Debug.WriteLine($"Publishing to {Published?.GetInvocationList().Length} subscribers");

            if (Published == null)
                return;

            // Reverse to start with the most recent sub.
            // That way, when e.g. moving a document, we can let the related view model handle the action, if available.
            // Otherwise, the chance is high that we traverse the documents of the root item and all subfolders
            // of the loaded folders until we find the correct document.
            List<Delegate> subs = Published.GetInvocationList().ToList();
            subs.Reverse();

            foreach (EventHandler<DataPublishedEventArgs> handler in subs)
            {
                handler.Invoke(this, args);
            }
        }
    }
}

