using Athena.DataModel;

namespace Athena.UI
{
    using DataModel.Core;

    internal interface IRootFolderService
    {
        /// <summary>
        /// Gets the root folder.
        /// </summary>
        /// <returns></returns>
        FolderViewModel GetRootFolder();

        /// <summary>
        /// Sets the root folder.
        /// </summary>
        /// <param name="folder"></param>
        void SetRootFolder(Folder folder);
    }
}
