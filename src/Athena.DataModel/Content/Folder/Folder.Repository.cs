using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder
    {
        /// <summary>
        /// Adds the <paramref name="page"/> to this <see cref="Folder"/>.
        /// </summary>
        /// <param name="page"></param>
        public void AddPage(Page page)
        {
            Pages.Add(page);
        }
        
        /// <summary>
        /// Reads all pages of this folder.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Page> ReadAllPages(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAllPages(context, this);
        }

        /// <summary>
        /// Reads all available folders.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<Folder> ReadAll(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAll(context);
        }
        
        /// <summary>
        /// Saves this <see cref="Folder"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Save(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().Save(context, this);
        }

        /// <summary>
        /// Deletes this <see cref="Folder"/>.
        /// </summary>
        /// <param name="context"></param>
        public void Delete(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().Delete(context, this);
        }

        /// <summary>
        /// Reads the <see cref="Folder"/> with the given <paramref name="folderKey"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folderKey"></param>
        /// <returns></returns>
        public static Folder Read(IContext context, FolderKey folderKey)
        {
            return DataStore.Resolve<IFolderRepository>().Read(context, folderKey);
        }
    }
}
