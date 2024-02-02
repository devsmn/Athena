using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder
    {
        public void AddPage(Page page)
        {
            this.Pages.Add(page);
        }
        
        public IEnumerable<Page> ReadAllPages(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAllPages(context, this);
        }

        public static IEnumerable<Folder> ReadAll(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAll(context);
        }
        
        public void Save(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().Save(context, this);
        }

        public void Delete(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().Delete(context, this);
        }

        public static Folder Read(IContext context, FolderKey folderKey)
        {
            return DataStore.Resolve<IFolderRepository>().Read(context, folderKey);
        }
    }
}
