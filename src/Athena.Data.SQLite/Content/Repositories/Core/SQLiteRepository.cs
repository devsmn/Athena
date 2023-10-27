using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Data.SQLite
{
    internal class SQLiteRepository
    {
        private readonly Lazy<SQLiteAsyncConnection> databaseDeferrer
            = new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(Defines.DatabasePath, Defines.Flags));

        protected SQLiteAsyncConnection Database
        {
            get { return databaseDeferrer.Value; }
        }

        /// <summary>
        /// Runs the given <paramref name="script"/>.
        /// </summary>
        /// <param name="script"></param>
        protected async Task RunScriptAsync(string script)
        {
            using (Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(script))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string content = await reader.ReadToEndAsync();
                    await this.Database.ExecuteAsync(content);
                }
            }
        }

        /// <summary>
        /// Reads the given <paramref name="file"/>.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected static async Task<string> ReadResouceAsync(string file)
        {
            using (Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(file))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        protected static string ReadResource(string file)
        {
            return ReadResouceAsync(file)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
        }
    }
}
