using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.UI;

namespace Athena.UI
{
    using Athena.DataModel;

    public class DataRequestService : IDataBrokerService
    {
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
                    // throw task.Exception;
                }
            },
               default,
               TaskContinuationOptions.OnlyOnFaulted,
               TaskScheduler.FromCurrentSynchronizationContext());
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

                    default:
                        throw new InvalidOperationException();
                }

            }

            Published?.Invoke(this, args);
        }

        public event EventHandler<DataPublishedEventArgs> Published;

        public event EventHandler PublishStarted;
    }
}

