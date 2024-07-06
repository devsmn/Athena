using Athena.DataModel.Core;

namespace Athena.UI
{
    public interface IVisualModel<in TEntity>
        where TEntity : Entity
    {
        int Id { get; }
        void Edit(TEntity entity);
        
    }
}
