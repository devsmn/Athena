using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Folder
    {
        /// <summary>
        /// Adds the given <paramref name="folder"/> to this <see cref="Folder"/>.
        /// </summary>
        /// <param name="folder"></param>
        public void AddFolder(Folder folder)
        {
            Folders.Add(folder);
        }

        /// <summary>
        /// Adds the given <see cref="Document"/> to this <see cref="Folder"/>.
        /// </summary>
        /// <param name="document"></param>
        public void AddDocument(Document document)
        {
            Documents.Add(document);
        }

        /// <summary>
        /// Reads all subfolders.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Folder> ReadAllFolders(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAllFolders(context, this);
        }

        /// <summary>
        /// Reads all documents.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Document> ReadAllDocuments(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAllDocuments(context, this);
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
        /// Creates the root folder.
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void CreateRoot(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().CreateRoot(context);
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
        /// Reads the <see cref="Folder"/> with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Folder Read(IContext context, IntegerEntityKey key)
        {
            return DataStore.Resolve<IFolderRepository>().Read(context, key);
        }
    }
}
