using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IPageRepository : IAthenaRepository
    {
        /// <summary>
        /// Adds the given <paramref name="tag"/> to this <see cref="Page"/>.
        /// </summary>
        /// <param name="tag"></param>
        void AddTag(Tag tag);

        /// <summary>
        /// Saves the given <paramref name="page"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="page"></param>
        void Save(IContext context, Page page);

        /// <summary>
        /// Reads the <see cref="Page"/> with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Page Read(IContext context, PageKey key);
        
        /// <summary>
        /// Deletes the given <paramref name="page"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="page"></param>
        void Delete(IContext context, Page page);
        
        /// <summary>
        /// Reads all pages.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<Page> ReadAll(IContext context);
    }
}
