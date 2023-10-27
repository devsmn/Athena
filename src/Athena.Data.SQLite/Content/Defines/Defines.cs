using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.SQLite
{
    internal static class Defines
    {
        public const string DatabaseFilename = "athena_rev_0004.db3";

        public const SQLiteOpenFlags Flags =
        // open the databaseDeferrer in read/write mode
        SQLiteOpenFlags.ReadWrite |
        // create the databaseDeferrer if it doesn't exist
        SQLiteOpenFlags.Create |
        // enable multi-threaded databaseDeferrer access
        SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }
}
