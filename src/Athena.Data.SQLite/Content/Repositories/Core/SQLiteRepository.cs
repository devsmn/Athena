using SQLite;
using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.Data.SQLite
{
    internal class SQLiteRepository
    {
        private readonly Lazy<SQLiteAsyncConnection> databaseDeferrer
            = new Lazy<SQLiteAsyncConnection>(() =>
            {
                Debug.WriteLine("DB AT at: " + Defines.DatabasePath);
                return new SQLiteAsyncConnection(Defines.DatabasePath, Defines.Flags);
            });

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

        protected TResult Audit<TResult>(IContext context, Func<TResult> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return default;
        }

        protected void Audit(IContext context, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }
    }
}
