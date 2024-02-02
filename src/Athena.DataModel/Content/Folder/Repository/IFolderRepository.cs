using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IFolderRepository : IAthenaRepository
    {
        void AddPage(IContext context, Folder folder, Page page);
        IEnumerable<Folder> ReadAll(IContext context);
        IEnumerable<Page> ReadAllPages(IContext context, Folder folder);
        void Save(IContext context, Folder folder);
        void Delete(IContext context, Folder folder);
        Folder Read(IContext context, FolderKey key);
    }
}
