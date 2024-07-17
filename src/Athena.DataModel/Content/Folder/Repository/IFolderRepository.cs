using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IFolderRepository : IAthenaRepository
    {
        /// <summary>
        /// Adds the given <paramref name="subFolder"/> to this <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <param name="subFolder"></param>
        void AddFolder(IContext context, Folder folder, Folder subFolder);

        /// <summary>
        /// Reads all documents of the given <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<Document> ReadAllDocuments(IContext context, Folder folder);

        /// <summary>
        /// Counts all folders.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        int CountAll(IContext context);

        /// <summary>
        /// Creates the root folder.
        /// </summary>
        /// <param name="context"></param>
        void CreateRoot(IContext context);

        /// <summary>
        /// Reads all subfolders.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<Folder> ReadAllFolders(IContext context, Folder folder);

        /// <summary>
        /// Saves the given <paramref name="folder"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <param name="saveOptions"></param>
        void Save(IContext context, Folder folder, FolderSaveOptions saveOptions = FolderSaveOptions.All);

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
        Folder Read(IContext context, IntegerEntityKey key);
    }
}
