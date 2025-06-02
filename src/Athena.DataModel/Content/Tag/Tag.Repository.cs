using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Tag
    {
        /// <summary>
        /// Reads all available tags.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<Tag> ReadAll(IContext context)
        {
            return DataStore.Resolve<ITagRepository>().ReadAll(context);
        }

        /// <summary>
        /// Saves this <see cref="Tag"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Save(IContext context)
        {
            DataStore.Resolve<ITagRepository>().Save(context, this);
        }

        /// <summary>
        /// Deletes this <see cref="Tag"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Delete(IContext context)
        {
            DataStore.Resolve<ITagRepository>().Delete(context, this);
        }
    }
}
