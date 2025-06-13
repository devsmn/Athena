using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    /// <summary>
    /// Provides common sqlite specific functionality. 
    /// </summary>
    internal class SqliteRepository
    {
        public bool IsValid { get; private set; }

        private readonly SQLiteAsyncConnection _database;

        protected SQLiteAsyncConnection Database
        {
            get
            {
                if (!IsValid)
                    throw new InvalidCipherException();

                return _database;
            }
        }

        public SqliteRepository(string cipher)
        {
            SQLiteConnectionString connectionString = new SQLiteConnectionString(Defines.DatabasePath, Defines.Flags, true, key: cipher);
            _database = new SQLiteAsyncConnection(connectionString);
        }

        protected async Task ValidateConnection()
        {
            try
            {
                // The only way to validate whether the cipher was correct is to execute a statement.
                string table = await _database.ExecuteScalarAsync<string>("SELECT name FROM sqlite_master WHERE type='table' and name='META';");
                IsValid = !string.IsNullOrEmpty(table);
            }
            catch (Exception ex)
            {
                IsValid = false;
            }

            if (!IsValid)
                throw new InvalidCipherException();
        }

        /// <summary>
        /// Runs the given <paramref name="script"/>.
        /// </summary>
        /// <param name="script"></param>
        protected async Task RunScriptAsync(string script)
        {
            await using (Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(script))
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
        public static async Task<string> ReadResourceAsync(string file)
        {
            await using (Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(file))
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

        /// <summary>
        /// Executes the given <paramref name="action"/> for the provided <paramref name="commandText"/> while
        /// observing exceptions.
        /// <para>
        /// The transaction is managed by this method, i.e. it is rolled-back automatically on errors.</para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected TResult Audit<TResult>(IContext context, string commandText, Func<SQLiteCommand, TResult> action)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                context.Log(new Exception("Command text is empty"));
                return default;
            }


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

        /// <summary>
        /// Creates an <see cref="SQLiteCommand"/> based on the provided <paramref name="commandText"/>
        /// and executes the given <paramref name="action"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
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
