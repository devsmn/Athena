
using Athena.DataModel.Core;
using SQLite;
using System.Diagnostics;

namespace Athena.Data.SQLite
{
    using Athena.DataModel;

    /// <summary>
    /// Provides the SQLite implementation of <see cref="IFolderRepository"/>.
    /// </summary>
    internal class SqLiteFolderRepository : SqLiteRepository, IFolderRepository
    {
        private string _readFolderSql;
        private string _deleteFolderSql;
        private string _readFolderPageSql;
        private string _insertFolderSql;
        private string _folderPageInsert;
        private string _folderUpdateSql;

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_FOLDER.sql");
            await RunScriptAsync("CREATE_TABLE_FOLDER_PAGE.sql");

            _insertFolderSql = await ReadResourceAsync("FOLDER_INSERT.sql");
            _readFolderSql = await ReadResourceAsync("FOLDER_READ.sql");
            _folderPageInsert = await ReadResourceAsync("FOLDER_PAGE_INSERT.sql");
            _readFolderPageSql = await ReadResourceAsync("FOLDER_PAGE_READ.sql");
            _folderUpdateSql = await ReadResourceAsync("FOLDER_UPDATE.sql");
            _deleteFolderSql = await ReadResourceAsync("FOLDER_DELETE.sql");

            Debug.Assert(!string.IsNullOrEmpty(_insertFolderSql));
            Debug.Assert(!string.IsNullOrEmpty(_readFolderSql));
            Debug.Assert(!string.IsNullOrEmpty(_folderPageInsert));
            Debug.Assert(!string.IsNullOrEmpty(_readFolderPageSql));
            Debug.Assert(!string.IsNullOrEmpty(_folderUpdateSql));

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

                SQLiteCommand command = connection.CreateCommand(this._folderPageInsert);

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
            return Audit<IEnumerable<Folder>>(
                context,
                _readFolderSql,
                command =>
                {
                    command.Bind("@FD_ref", -1);
                    return command.ExecuteQuery<Folder>();
                });

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

                var command = connection.CreateCommand(this._readFolderPageSql);

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

        public void Delete(IContext context, Folder folder)
        {
            Audit(
                context,
                _deleteFolderSql,
                command =>
                {
                    command.Bind("@FD_ref", folder.Key.Id);
                    command.ExecuteNonQuery();

                    // We currently can't resolve this via CASCADE because the Chapters of the documents are not referenced directly.
                    foreach (var page in folder.Pages)
                    {
                        page.Delete(context);
                    }

                });
        }

        public Folder Read(IContext context, FolderKey key)
        {
            return Audit(
                context,
                _readFolderSql,
                command =>
                {
                    command.Bind("@FD_ref", key.Id);
                    return command.ExecuteQuery<Folder>()[0];
                });
        }

        private void Update(IContext context, Folder folder)
        {
            Audit(
                context,
                _folderUpdateSql,
                command =>
                {
                    command.Bind("@FD_ref", folder.Key.Id);
                    command.Bind("@FD_name", folder.Name);
                    command.Bind("@FD_comment", folder.Comment.EmptyIfNull());
                    command.Bind("@FD_isPinnedInt", folder.IsPinnedInt);
                    command.ExecuteNonQuery();

                    foreach (var page in folder.Pages)
                    {
                        AddPage(context, folder, page);
                    }
                });
        }



        private void Insert(IContext context, Folder folder)
        {
            try
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this._insertFolderSql);

                command.Bind("@FD_name", folder.Name.EmptyIfNull());
                command.Bind("@FD_comment", folder.Comment.EmptyIfNull());
                command.Bind("@FD_isPinnedInt", folder.IsPinnedInt);
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
