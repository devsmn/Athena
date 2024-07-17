namespace Athena.DataModel
{
    [Flags]
    public enum FolderSaveOptions
    {
        Folder = 0,
        Documents = 1,
        SubFolders = 2,
        All = Folder | Documents | SubFolders
    }
}
