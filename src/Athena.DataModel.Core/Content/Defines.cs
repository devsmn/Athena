namespace Athena.DataModel.Core
{
    public static class Defines
    {
        public const string UnsafeDatabaseFileName = "athena_rc8.db3";
        public const string DatabaseFileName = "athena_encr.db3";

        /// <summary>
        /// Gets the path of the unencrypted, old database.
        /// </summary>
        public static string UnsafeDatabasePath
        {
            get
            {
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, UnsafeDatabaseFileName);
            }
        }

        /// <summary>
        /// Gets the path of the encrypted database. 
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFileName);
            }
        }
    }
}
