using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.UI
{
    using Athena.DataModel;

    internal class Store<TEntity>
    {
        private readonly Dictionary<EntityKey, Entity> _entities;

        public Store()
        {
            _entities = new Dictionary<EntityKey, Entity>();
        }

        public void Add(EntityKey key, Entity entity)
        {
            _entities.Add(key, entity);
        }

        public Entity Get(EntityKey key)
        {
            _entities.TryGetValue(key, out var entity);
            return entity;
        }

        public IEnumerable<Entity> GetAll()
        {
            return _entities.Select(x => x.Value);
        }
    }

    public class DataRequestService : IDataBrokerService
    {
        private readonly Dictionary<Type, Store<Entity>> _entities;

        public DataRequestService()
        {
            _entities = new Dictionary<Type, Store<Entity>>();
        }

        public void PrepareForLoading()
        {
            RaisePublishStarted();
        }

        public void RaiseAppInitialized()
        {
            AppInitialized?.Invoke(this, EventArgs.Empty);
        }

        public TEntity Request<TEntity>(IContext context, EntityKey key)
            where TEntity : Entity
        {
            if (!_entities.TryGetValue(typeof(TEntity), out var store))
            {
                store = new Store<Entity>();
                _entities.Add(typeof(TEntity), store);
            }

            var result = store.Get(key);

            return result as TEntity;
        }

        public IEnumerable<TEntity> Request<TEntity>(IContext context)
        {
            if (!_entities.TryGetValue(typeof(TEntity), out var store))
            {
                store = new Store<Entity>();
                _entities.Add(typeof(TEntity), store);
            }

            var result = store.GetAll();

            return result as IEnumerable<TEntity>;
        }


        public void Publish<TEntity>(IContext context, TEntity entity, UpdateType type, EntityKey parentReference)
            where TEntity : Entity
        {
            Publish(context, new List<TEntity> { entity }, type, parentReference);
        }

        public void Publish<TEntity>(IContext context, TEntity entity, UpdateType type)
            where TEntity : Entity
        {
            Publish(context, entity, type, null);
        }

        public void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type) where TEntity : Entity
        {
            Publish(context, entities, type, null);
        }

        public void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type, EntityKey parentReference) where TEntity : Entity
        {
            var syncContext = TaskScheduler.Current;

            Task.Run(() =>
            {
                RaisePublishStarted();

                IList<RequestUpdate<TEntity>> updates = new List<RequestUpdate<TEntity>>();

                foreach (var entity in entities)
                {
                    updates.Add(new RequestUpdate<TEntity>(entity, type, parentReference));
                }

                RaisePublished(updates);
            }).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    context.Log(task.Exception);
                    throw task.Exception;
                }
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

        public event EventHandler<DataPublishedEventArgs> Published;
        public event EventHandler AppInitialized;
        public event EventHandler PublishStarted;
    }
}

