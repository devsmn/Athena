using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface ITagRepository : IAthenaRepository
    {
        IEnumerable<Tag> ReadAll(IContext context);
        void Save(IContext context, Tag tag);
        void Delete(IContext context, Tag tag);
    }
}
