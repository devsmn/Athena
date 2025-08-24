using SQLite;

namespace Athena.Data.SQLite
{
    internal static class SqlFlags
    {
        public static string Key { get; set; }

        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create | 
            SQLiteOpenFlags.SharedCache;
    }
}
