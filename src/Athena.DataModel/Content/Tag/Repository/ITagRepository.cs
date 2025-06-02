using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface ITagRepository : IAthenaRepository
    {
        /// <summary>
        /// Reads all available tags.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<Tag> ReadAll(IContext context);

        /// <summary>
        /// Saves the given <paramref name="tag"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        void Save(IContext context, Tag tag);

        /// <summary>
        /// Deletes the given <paramref name="tag"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        void Delete(IContext context, Tag tag);
    }
}
