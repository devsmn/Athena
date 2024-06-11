using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.UI
{
    using Athena.DataModel;

    public class DefaultDataBrokerService : IDataBrokerService
    {
        public event EventHandler<DataPublishedEventArgs> Published;
        public event EventHandler AppInitialized;
        public event EventHandler PublishStarted;

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
        public void Publish<TEntity>(IContext context, TEntity entity, UpdateType type, EntityKey parentReference)
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
        public void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type, EntityKey parentReference) where TEntity : Entity
        {
            var syncContext = TaskScheduler.Current;

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
                default,
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

            foreach (var update in updates)
            {
                switch (update.Entity)
                {
                    case Document:
                        args.Documents.Add(update as RequestUpdate<Document>);
                        break;

                    case Page:
                        args.Pages.Add(update as RequestUpdate<Page>);
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

            foreach (EventHandler<DataPublishedEventArgs> handler in Published?.GetInvocationList())
            {
                handler.Invoke(this, args);
            }
        }
    }
}

