using Athena.DataModel;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.Messaging;

namespace Athena.UI
{
    public class DefaultRootFolderService : IRootFolderService
    {
        private FolderViewModel _rootFolder;

        /// <inheritdoc />  
        public FolderViewModel GetRootFolder()
        {
            return _rootFolder;
        }

        /// <inheritdoc />  
        public void SetRootFolder(Folder folder)
        {
            _rootFolder = new FolderViewModel(folder);
        }
    }
}

