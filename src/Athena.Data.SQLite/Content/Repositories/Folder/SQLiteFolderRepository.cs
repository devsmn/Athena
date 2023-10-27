
using Athena.DataModel.Core;
using SQLite;
using System.Diagnostics;

namespace Athena.Data.SQLite
{
    using Athena.DataModel;

    /// <summary>
    /// Provides the SQLite implementation of <see cref="IFolderRepository"/>.
    /// </summary>
    internal class SQLiteFolderRepository : SQLiteRepository, IFolderRepository
    {
        private string readFolderSql;
        private string readFolderPageSql;
        private string insertFolderSql;
        private string folderPageInsert;
        private string folderUpdateSql;

        public async Task<bool> InitializeAsync()
        {
            await this.RunScriptAsync("CREATE_TABLE_FOLDER.sql");
            await this.RunScriptAsync("CREATE_TABLE_FOLDER_PAGE.sql");

            insertFolderSql = await ReadResouceAsync("FOLDER_INSERT.sql");
            readFolderSql = await ReadResouceAsync("FOLDER_READ.sql");
            folderPageInsert = await ReadResouceAsync("FOLDER_PAGE_INSERT.sql");
            readFolderPageSql = await ReadResouceAsync("FOLDER_PAGE_READ.sql");
            folderUpdateSql = await ReadResouceAsync("FOLDER_UPDATE.sql");

            Debug.Assert(!string.IsNullOrEmpty(insertFolderSql));
            Debug.Assert(!string.IsNullOrEmpty(readFolderSql));
            Debug.Assert(!string.IsNullOrEmpty(folderPageInsert));
            Debug.Assert(!string.IsNullOrEmpty(readFolderPageSql));
            Debug.Assert(!string.IsNullOrEmpty(folderUpdateSql));

            return await Task.FromResult(true);
        }

        public void AddPage(IContext context, Folder folder, Page page)
        {
            try
            {
                if (page.Key.Id == PageKey.TemporaryId)
                {
                    page.Save(context);
                }
                else
                {
                    return; // TODO
                }

                var connection = this.Database.GetConnection();

                Debug.Assert(page.Key.Id != PageKey.TemporaryId);

                SQLiteCommand command = connection.CreateCommand(this.folderPageInsert);

                command.Bind("@FD_ref", folder.Key.Id);
                command.Bind("@PG_ref", page.Key.Id);
                command.Bind("@FDPG_creationDate", DateTime.UtcNow);
                command.Bind("@FDPG_modDate", DateTime.UtcNow);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }

        public IEnumerable<Folder> ReadAll(IContext context)
        {
            try
            {
                var connection = this.Database.GetConnection();
                var command = connection.CreateCommand(this.readFolderSql);
                command.Bind("@FD_ref", -1);

                var folders = command.ExecuteQuery<Folder>();

                foreach (var folder in folders)
                {
                    folder.ReadAllPages(context);
                }

                return folders;
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Folder>();
        }

        public IEnumerable<Page> ReadAllPages(IContext context, Folder folder)
        {
            try
            {
                if (folder.Key == null || folder.Key.Id == FolderKey.TemporaryId)
                {
                    return Enumerable.Empty<Page>();
                }

                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readFolderPageSql);

                command.Bind("@FD_ref", folder.Key.Id);

                return command.ExecuteQuery<Page>();
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Page>();
        }

        public void Save(IContext context, Folder folder)
        {
            if (folder.Key == null || folder.Key.Id == IntegerEntityKey.TemporaryId)
            {
                Insert(context, folder);
            }
            else
            {
                Update(context, folder);
            }
        }

        private void Update(IContext context, Folder folder)
        {
            try
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.folderUpdateSql);

                command.Bind("@FD_name", folder.Name);
                command.Bind("@FD_comment", folder.Comment);
                command.Bind("@FD_thumbnail", Convert.ToBase64String(folder.Thumbnail ?? new byte[0]));
                command.ExecuteNonQuery();

                foreach (var page in folder.Pages)
                {
                    AddPage(context, folder, page);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }

        private void Insert(IContext context, Folder folder)
        {
            try
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.insertFolderSql);

                command.Bind("@FD_name", folder.Name.EmptyIfNull());
                command.Bind("@FD_comment", folder.Comment.EmptyIfNull());
                command.Bind("@FD_thumbnail", Convert.ToBase64String(folder.Thumbnail ?? Array.Empty<byte>()));
                command.Bind("@FD_creationDate", folder.CreationDate);
                command.Bind("@FD_modDate", folder.ModDate);
                command.ExecuteNonQuery();

                folder.SetKey(new FolderKey((int)SQLite3.LastInsertRowid(connection.Handle)));

                foreach (var page in folder.Pages)
                {
                    AddPage(context, folder, page);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }
    }
}
