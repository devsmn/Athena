using SQLite;

namespace Athena.Data.SQLite
{
    internal static class Defines
    {
        public static string Key { get; set; }
        public const string UnsafeDatabaseFileName = "athena_rc8.db3";
        public const string DatabaseFileName = "athena_enc.db3";

        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Gets the path of the unencrypted, old database.
        /// </summary>
        public static string UnsafeDatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFileName);
            }
        }
    }
}
