using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;

namespace Athena.UI
{
    using Athena.DataModel.Core;

    internal interface IDataBrokerService
    {
        void Publish<TEntity>(IContext context, TEntity entity, UpdateType type) where TEntity : Entity;
        void Publish<TEntity>(IContext context, TEntity entity, UpdateType type, EntityKey parentReference) where TEntity : Entity;
        void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type) where TEntity : Entity;
        void Publish<TEntity>(IContext context, IEnumerable<TEntity> entities, UpdateType type, EntityKey parentReference) where TEntity : Entity;

        event EventHandler PublishStarted;
        event EventHandler<DataPublishedEventArgs> Published;
    }
}
