using SQLite;
using System.Diagnostics;
using Athena.DataModel.Core;

namespace Athena.Data.SQLite
{
    internal class SqLiteRepository
    {
        private readonly Lazy<SQLiteAsyncConnection> _databaseDeferrer
            = new(() => new SQLiteAsyncConnection(Defines.DatabasePath, Defines.Flags));

        protected SQLiteAsyncConnection Database
        {
            get { return _databaseDeferrer.Value; }
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
                    await Database.ExecuteAsync(content);
                }
            }
        }

        /// <summary>
        /// Reads the given <paramref name="file"/>.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected static async Task<string> ReadResourceAsync(string file)
        {
            using (Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(file))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        [Obsolete("Should not be used because no transaction handling")]
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

        protected TResult Audit<TResult>(IContext context, string commandText, Func<SQLiteCommand, TResult> action)
        {
            SQLiteConnection connection = Database.GetConnection();
            bool ownTransaction = !connection.IsInTransaction;

            try
            {
                if (ownTransaction)
                {
                    connection.BeginTransaction();
                }

                return action(connection.CreateCommand(commandText));
            }
            catch (Exception ex)
            {
                if (ownTransaction)
                {
                    connection.Rollback();
                }

                context.Log(ex);
            }
            finally
            {
                if (ownTransaction)
                {
                    connection.Commit();
                }
            }

            return default;
        }

        [Obsolete("Should not be used because no transaction handling")]
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

        protected void Audit(IContext context, string commandText, Action<SQLiteCommand> action)
        {
            SQLiteConnection connection = Database.GetConnection();
            bool ownTransaction = !connection.IsInTransaction;

            try
            {
                if (ownTransaction)
                {
                    connection.BeginTransaction();
                }

                action(connection.CreateCommand(commandText));

                if (ownTransaction)
                {
                    connection.Commit();
                }
            }
            catch (Exception ex)
            {
                if (ownTransaction)
                {
                    connection.Rollback();
                }

                context.Log(ex);
            }
        }
    }
}
