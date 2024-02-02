using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Chapter
    {
        public static IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            return DataStore.Resolve<IChapterRepository>().Search(context, pattern);
        }

        public void Save(IContext context)
        {
            DataStore.Resolve<IChapterRepository>().Save(context, this);
        }

        public static void Delete(IContext context, int documentRef)
        {
            DataStore.Resolve<IChapterRepository>().Delete(context, documentRef);
        }

        public void ReadDocument(IContext context)
        {
            Document = Document.Read(context, new DocumentKey(Convert.ToInt32(this.DocumentId)));
        }

        public void ReadPage(IContext context)
        {
            Page = Page.Read(context, new PageKey(Convert.ToInt32(this.PageId)));
        }

        public void ReadFolder(IContext context)
        {
            Folder = Folder.Read(context, new FolderKey(Convert.ToInt32(this.FolderId)));
        }
    }
}
