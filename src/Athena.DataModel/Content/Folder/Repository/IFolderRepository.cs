using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IFolderRepository : IAthenaRepository
    {
        /// <summary>
        /// Adds the given <paramref name="page"/> to the <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <param name="page"></param>
        void AddPage(IContext context, Folder folder, Page page);

        /// <summary>
        /// Reads all folders.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<Folder> ReadAll(IContext context);

        /// <summary>
        /// Reads all pages of the given <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<Page> ReadAllPages(IContext context, Folder folder);

        /// <summary>
        /// Saves the given <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        void Save(IContext context, Folder folder);

        /// <summary>
        /// Deletes the given <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        void Delete(IContext context, Folder folder);

        /// <summary>
        /// Reads the folder with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Folder Read(IContext context, FolderKey key);
    }
}
