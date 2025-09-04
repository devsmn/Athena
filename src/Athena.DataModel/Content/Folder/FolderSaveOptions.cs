namespace Athena.DataModel
{
    /// <summary>
    /// Provides possible options for saving a <see cref="Folder"/> and its children.
    /// <para>
    /// The flags can be combined.</para>
    /// </summary>
    [Flags]
    public enum FolderSaveOptions
    {
        /// <summary>
        /// Save changes to the folder itself (e.g. name).
        /// </summary>
        Folder = 0,

        /// <summary>
        /// Only save changes to the related documents.
        /// </summary>
        Documents = 1,

        /// <summary>
        /// Includes any changes made to subfolders.
        /// </summary>
        SubFolders = 2,

        /// <summary>
        /// Include all changes.
        /// </summary>
        All = Folder | Documents | SubFolders
    }
}
