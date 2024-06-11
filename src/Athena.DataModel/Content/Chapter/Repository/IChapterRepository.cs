using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IChapterRepository : IAthenaRepository
    {
        /// <summary>
        /// Searches for the given <paramref name="pattern"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        IEnumerable<Chapter> Search(IContext context, string pattern);

        /// <summary>
        /// Saves the given <paramref name="chapter"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="chapter"></param>
        void Save(IContext context, Chapter chapter);

        /// <summary>
        /// Deletes the <see cref="Chapter"/> with the given <paramref name="documentRef"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="documentRef"></param>
        void Delete(IContext context, int documentRef);
    }
}
