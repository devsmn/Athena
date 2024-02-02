using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IPageRepository : IAthenaRepository
    {
        void AddTag(Tag tag);

        void Save(IContext context, Page page);

        Page Read(IContext context, PageKey key);
        
        void Delete(IContext context, Page page);
        
        IEnumerable<Page> ReadAll(IContext context);
    }
}
