using SQLite;

namespace Athena.Data.SQLite
{
    using DataModel;
    using DataModel.Core;

    /// <summary>
    /// Provides the SQLite implementation of <see cref="IFolderRepository"/>.
    /// </summary>
    internal class SqliteFolderRepository : SqliteRepository, IFolderRepository
    {
        private string _readFolderSql;
        private string _deleteFolderSql;
        private string _readFolderFolderSql;
        private string _insertFolderSql;
        private string _insertFolderFolderSql;
        private string _insertFolderDocumentSql;
        private string _folderUpdateSql;
        private string _readFolderDocsSql;
        private string _createRootFolderSql;
        private string _countAllSql;

        public async Task<bool> InitializeAsync(IContext context)
        {
            context.Log("Initializing folder repository");

            _insertFolderSql = await ReadResourceAsync("FOLDER_INSERT.sql");
            _readFolderSql = await ReadResourceAsync("FOLDER_READ.sql");
            _insertFolderFolderSql = await ReadResourceAsync("FOLDER_FOLDER_INSERT.sql");
            _readFolderFolderSql = await ReadResourceAsync("FOLDER_FOLDER_READ.sql");
            _folderUpdateSql = await ReadResourceAsync("FOLDER_UPDATE.sql");
            _deleteFolderSql = await ReadResourceAsync("FOLDER_DELETE.sql");
            _readFolderDocsSql = await ReadResourceAsync("FOLDER_DOC_READ.sql");
            _createRootFolderSql = await ReadResourceAsync("FOLDER_CREATE_ROOT.sql");
            _insertFolderDocumentSql = await ReadResourceAsync("FOLDER_DOC_INSERT.sql");
            _countAllSql = await ReadResourceAsync("FOLDER_COUNT.sql");

            return await Task.FromResult(true);
        }

        public void RegisterPatches(IContext context, ICompatibilityService compatService)
        {
            compatService.RegisterPatch<SqliteFolderRepository>(new(1, CreateTables));
        }

        private async Task CreateTables(IContext context)
        {
            context.Log("Creating folder data storage");

            await RunScriptAsync("CREATE_TABLE_FOLDER.sql");
            await RunScriptAsync("CREATE_TABLE_FOLDER_FOLDER.sql");
            await RunScriptAsync("CREATE_TABLE_FOLDER_DOC.sql");
        }

        public async Task ExecutePatches(IContext context, ICompatibilityService compatService)
        {
            var patches = compatService.GetPatches<SqliteFolderRepository>();

            foreach (var pat in patches)
            {
                await pat.PatchAsync(context);
            }
        }

        public void AddDocument(IContext context, Folder folder, Document document)
        {
            try
            {
                if (document.Key.Id == IntegerEntityKey.TemporaryId)
                {
                    document.Save(context);
                }
                else
                {
                    return;
                    // TODO
                }

                var connection = Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(_insertFolderDocumentSql);

                command.Bind("@FD_ref", folder.Id);
                command.Bind("@DOC_ref", document.Id);
                command.Bind("@FDDOC_creationDate", DateTime.UtcNow);
                command.Bind("@FDDOC_modDate", DateTime.UtcNow);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        public void AddFolder(IContext context, Folder parentFolder, Folder subFolder)
        {
            try
            {
                if (subFolder.Key.Id == IntegerEntityKey.TemporaryId)
                {
                    subFolder.Save(context);
                }
                else
                {
                    return; // TODO
                }

                var connection = Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(_insertFolderFolderSql);

                command.Bind("@FD_refParent", parentFolder.Key.Id);
                command.Bind("@FD_ref", subFolder.Key.Id);
                command.Bind("@FDFD_creationDate", DateTime.UtcNow);
                command.Bind("@FDFD_modDate", DateTime.UtcNow);
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        public IEnumerable<Document> ReadAllDocuments(IContext context, Folder folder)
        {
            return Audit<IEnumerable<Document>>(
                context,
                _readFolderDocsSql,
                command =>
                {
                    command.Bind("@FD_ref", folder.Key.Id);
                    return command.ExecuteQuery<Document>();
                });
        }

        /// <inheritdoc />  
        public int CountAll(IContext context)
        {
            return Audit(
                context,
                _countAllSql,
                command => command.ExecuteScalar<int>());
        }

        public IEnumerable<Folder> ReadAllFolders(IContext context, Folder folder)
        {
            try
            {
                if (folder.Key == null || folder.Key.Id == IntegerEntityKey.TemporaryId)
                {
                    return Enumerable.Empty<Folder>();
                }

                var connection = Database.GetConnection();
                var command = connection.CreateCommand(_readFolderFolderSql);

                command.Bind("@FD_refParent", folder.Key.Id);

                return command.ExecuteQuery<Folder>();
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Folder>();
        }

        public void Save(IContext context, Folder folder, FolderSaveOptions folderOptions = FolderSaveOptions.All)
        {
            if (folder.Key == null || folder.Key.Id == IntegerEntityKey.TemporaryId)
            {
                Insert(context, folder);
            }
            else
            {
                Update(context, folder, folderOptions);
            }
        }

        /// <inheritdoc />  
        public void CreateRoot(IContext context)
        {
            Audit(
                context,
                _createRootFolderSql,
                command =>
                {
                    command.ExecuteNonQuery();
                });
        }

        /// <inheritdoc />  
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
                    foreach (var subFolder in folder.Folders)
                    {
                        subFolder.Delete(context);
                    }

                    foreach (var document in folder.Documents)
                    {
                        document.Delete(context);
                    }
                });
        }

        public Folder Read(IContext context, IntegerEntityKey key)
        {
            return Audit(
                context,
                _readFolderSql,
                command =>
                {
                    command.Bind("@FD_ref", key.Id);

                    var folders = command.ExecuteQuery<Folder>();

                    if (folders != null && folders.Count > 0)
                        return folders[0];

                    return null;
                });
        }

        private void Update(IContext context, Folder folder, FolderSaveOptions saveOptions)
        {
            Audit(
                context,
                _folderUpdateSql,
                command =>
                {
                    if (folder.Key != IntegerEntityKey.Root)
                    {
                        command.Bind("@FD_ref", folder.Key.Id);
                        command.Bind("@FD_name", folder.Name);
                        command.Bind("@FD_comment", folder.Comment.EmptyIfNull());
                        command.Bind("@FD_isPinnedInt", folder.IsPinnedInt);
                        command.ExecuteNonQuery();
                    }

                    if (saveOptions.HasFlag(FolderSaveOptions.All) || saveOptions.HasFlag(FolderSaveOptions.SubFolders))
                    {
                        foreach (var subFolder in folder.Folders)
                        {
                            AddFolder(context, folder, subFolder);
                        }
                    }

                    if (saveOptions.HasFlag(FolderSaveOptions.All) || saveOptions.HasFlag(FolderSaveOptions.Documents))
                    {
                        foreach (var document in folder.Documents)
                        {
                            AddDocument(context, folder, document);
                        }
                    }
                });
        }



        private void Insert(IContext context, Folder folder)
        {
            try
            {
                var connection = Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(_insertFolderSql);

                command.Bind("@FD_name", folder.Name.EmptyIfNull());
                command.Bind("@FD_comment", folder.Comment.EmptyIfNull());
                command.Bind("@FD_isPinnedInt", folder.IsPinnedInt);
                command.Bind("@FD_creationDate", folder.CreationDate);
                command.Bind("@FD_modDate", folder.ModDate);
                command.ExecuteNonQuery();

                folder.Key = new IntegerEntityKey((int)SQLite3.LastInsertRowid(connection.Handle));

                foreach (var subFolder in folder.Folders)
                {
                    AddFolder(context, folder, subFolder);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }
    }
}
