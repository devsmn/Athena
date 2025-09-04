using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Chapter
    {
        /// <summary>
        /// Searches the <see cref="Chapter"/> with the given <paramref name="pattern"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            return DataStore.Resolve<IChapterRepository>().Search(context, pattern);
        }

        /// <summary>
        /// Saves this <see cref="Chapter"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Save(IContext context)
        {
            DataStore.Resolve<IChapterRepository>().Save(context, this);
        }

        /// <summary>
        /// Deletes the <see cref="Chapter"/> with the given <paramref name="documentRef"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="documentRef"></param>
        public static void Delete(IContext context, int documentRef)
        {
            DataStore.Resolve<IChapterRepository>().Delete(context, documentRef);
        }

        /// <summary>
        /// Reads the <see cref="Document"/>.
        /// </summary>
        /// <param name="context"></param>
        public void ReadDocument(IContext context)
        {
            Document = Document.Read(context, new IntegerEntityKey(Convert.ToInt32(DocumentId)));
        }

        /// <summary>
        /// Reads the <see cref="Folder"/>.
        /// </summary>
        /// <param name="context"></param>
        public void ReadFolder(IContext context)
        {
            if (!IsRoot)
                Folder = Folder.Read(context, new IntegerEntityKey(Convert.ToInt32(FolderId)));
        }
    }
}
