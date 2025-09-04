using System.Diagnostics;
using Athena.DataModel;
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    /// <summary>
    /// Provides the SQLite implementation of <see cref="IDocumentRepository"/>.
    /// </summary>
    internal class SqliteDocumentRepository : SqliteRepository, IDocumentRepository
    {
        private string _insertDocumentSql;
        private string _insertDocumentTagSql;
        private string _readDocumentSql;
        private string _readFolderDocSql;
        private string _readDocumentTagSql;
        private string _deleteDocumentSql;
        private string _deleteTagSql;
        private string _searchDocWithTagSql;
        private string _searchDocNoTagSql;
        private string _readDocumentPdfSql;
        private string _updateDocumentSql;
        private string _moveToPageSql;
        private string _moveToPageFtsSql;
        private string _readRecentSql;
        private string _countAllSql;
        private string _patchPdfSql;

        public SqliteDocumentRepository(string cipher) : base(cipher)
        {
        }

        public async Task<bool> InitializeAsync(IContext context)
        {
            await ValidateConnection();
            context.Log("Initializing document repository");

            _insertDocumentSql = await ReadResourceAsync("DOCUMENT_INSERT.sql");
            _insertDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_INSERT.sql");
            _readDocumentSql = await ReadResourceAsync("DOCUMENT_READ.sql");
            _readFolderDocSql = await ReadResourceAsync("FOLDER_DOC_READ.sql");
            _deleteDocumentSql = await ReadResourceAsync("DOCUMENT_DELETE.sql");
            _readDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_READ.sql");
            _deleteTagSql = await ReadResourceAsync("DOCUMENT_TAG_DELETE.sql");
            _searchDocWithTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_TAG.sql");
            _searchDocNoTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_NOTAG.sql");
            _readDocumentPdfSql = await ReadResourceAsync("DOCUMENT_READ_PDF.sql");
            _updateDocumentSql = await ReadResourceAsync("DOCUMENT_UPDATE.sql");
            _moveToPageSql = await ReadResourceAsync("DOCUMENT_MOVE.sql");
            _moveToPageFtsSql = await ReadResourceAsync("DOCUMENT_FTS_MOVE.sql");
            _readRecentSql = await ReadResourceAsync("DOCUMENT_READ_RECENT.sql");
            _countAllSql = await ReadResourceAsync("DOCUMENT_COUNT.sql");
            _patchPdfSql = await ReadResourceAsync("DOCUMENT_UPDATE_PDF.sql");

            return await Task.FromResult(true);
        }

        public void RegisterPatches(IContext context, ICompatibilityService compatService)
        {
            compatService.RegisterPatch<SqliteDocumentRepository>(new(1, CreateTables));
            compatService.RegisterPatch<SqliteDocumentRepository>(new(55, PatchTables55));
        }

        public async Task ExecutePatches(IContext context, ICompatibilityService compatService)
        {
            IEnumerable<VersionPatch> patches = compatService.GetPatches<SqliteDocumentRepository>(context);

            foreach (VersionPatch pat in patches)
            {
                await pat.PatchAsync(context);
            }
        }

        private async Task CreateTables(IContext context)
        {
            context.Log("Creating document data storage");

            await RunScriptAsync("CREATE_TABLE_DOCUMENT.sql");
            await RunScriptAsync("CREATE_TABLE_DOCUMENT_TAG.sql");
        }

        private async Task PatchTables55(IContext context)
        {
            context.Log("Patch #55: Preparing sources");

            await Audit(
                context,
                _readDocumentSql,
                async command =>
                {
                    context.Log("Patch #55: compressing PDFs");
                    command.Bind("@DOC_ref", -1);
                    List<Document> documents = command.ExecuteQuery<Document>();

                    ICompressionService compressionService = Services.GetService<ICompressionService>();

                    foreach (Document document in documents)
                    {
                        context.Log($"Path #55: Patching PDF for document {document.Id}");
                        document.ReadPdf(context);

                        using (MemoryStream ms = new(document.Pdf))
                        {
                            document.Pdf = await compressionService.CompressAsync(ms);
                            PatchPdf(document);
                        }
                    }
                });

            context?.Log("Document PDF compression patch finished");
        }

        private void PatchPdf(Document doc)
        {
            Audit(
                null,
                _patchPdfSql,
                command =>
                {
                    command.Bind("@DOC_ref", doc.Id);
                    command.Bind("@DOC_pdf", doc.Pdf);
                });
        }

        /// <inheritdoc />  
        public Document Read(IContext context, IntegerEntityKey key)
        {
            return Audit(
                context,
                _readDocumentSql,
                command =>
                {
                    command.Bind("@DOC_ref", key.Id);
                    Document doc = command.ExecuteQuery<Document>()[0];

                    // Documents are decompressed via ReadPdf.
                    //doc.Pdf = compression.Decompress(doc.Pdf);

                    return doc;
                });
        }

        /// <inheritdoc />  
        public void DeleteTag(IContext context, Document document, Tag tag)
        {
            Audit(
                context,
                _deleteTagSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    command.Bind("@TAG_ref", tag.Id);

                    command.ExecuteNonQuery();
                });
        }

        /// <inheritdoc />  
        public void AddTag(IContext context, Document document, Tag tag)
        {
            Audit(
                context,
                _insertDocumentTagSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    command.Bind("@TAG_ref", tag.Id);
                    command.Bind("@DOCTAG_creationDate", DateTime.UtcNow);
                    command.Bind("@DOCTAG_modDate", DateTime.MinValue);

                    command.ExecuteNonQuery();
                });
        }

        /// <inheritdoc />  
        public IEnumerable<Tag> ReadTags(IContext context, Document document)
        {
            return Audit<IEnumerable<Tag>>(
                context,
                _readDocumentTagSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    return command.ExecuteQuery<Tag>();
                });
        }

        /// <inheritdoc />  
        [Obsolete]
        public IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFts)
        {
            documentName = string.IsNullOrEmpty(documentName) ? string.Empty : $"%{documentName}%";

            return Audit(context, () =>
            {
                IEnumerable<SearchResult> results = new List<SearchResult>();

                if (tags.Any())
                {
                    results = SearchWithTags(documentName, tags, useFts);
                }
                else
                {
                    results = SearchNoTags(documentName, useFts);
                }

                foreach (SearchResult result in results)
                {
                    result.Fill();
                }

                return results;
            });
        }

        /// <inheritdoc />  
        public byte[] ReadPdf(IContext context, Document document)
        {
            return Audit(
                context,
                _readDocumentPdfSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    byte[] data = command.ExecuteScalar<byte[]>();

                    ICompressionService compressionService = Services.GetService<ICompressionService>();
                    return compressionService.Decompress(data);
                });
        }

        private IEnumerable<SearchResult> SearchNoTags(string documentName, bool useFts)
        {
            SQLiteConnectionWithLock connection = Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(_searchDocNoTagSql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFts ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        private IEnumerable<SearchResult> SearchWithTags(string documentName, IEnumerable<Tag> tags, bool useFts)
        {
            string ids = string.Join(",", tags.Select(x => x.Id));
            string sql = _searchDocWithTagSql.Replace("<<__replace__>>", ids);

            SQLiteConnectionWithLock connection = Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(sql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFts ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        /// <inheritdoc />  
        public int CountAll(IContext context)
        {
            return Audit(
                context,
                _countAllSql,
                command => command.ExecuteScalar<int>());
        }

        /// <inheritdoc />  
        public void Delete(IContext context, Document document)
        {
            Audit(
                context,
                _deleteDocumentSql,
                command =>
                {
                    Debug.Assert(document.Key.Id != IntegerEntityKey.TemporaryId);

                    command.Bind("@DOC_ref", document.Key.Id);
                    command.ExecuteNonQuery();

                    Chapter.Delete(context, document.Key.Id);
                });
        }

        /// <inheritdoc />  
        public void Save(IContext context, Document document)
        {
            try
            {
                if (document.Key == null || document.Id == IntegerEntityKey.TemporaryId)
                {
                    InsertDocumentCore(context, document);
                }
                else
                {
                    UpdateDocumentCore(context, document);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }

        private void InsertDocumentCore(IContext context, Document document)
        {
            SQLiteConnectionWithLock connection = Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(_insertDocumentSql);

            ICompressionService compress = Services.GetService<ICompressionService>();
            byte[] pdf = compress.Compress(document.Pdf ?? Array.Empty<byte>());

            command.Bind("@DOC_name", document.Name.EmptyIfNull());
            command.Bind("@DOC_comment", document.Comment.EmptyIfNull());
            command.Bind("@DOC_pdf", pdf);
            command.Bind("@DOC_thumbnail", Convert.ToBase64String(document.Thumbnail ?? Array.Empty<byte>()));
            command.Bind("@DOC_creationDate", DateTime.UtcNow);
            command.Bind("@DOC_modDate", DateTime.UtcNow);
            command.Bind("@DOC_isPinned", document.IsPinnedInteger);

            command.ExecuteNonQuery();

            document.Key = new IntegerEntityKey((int)SQLite3.LastInsertRowid(connection.Handle));

            foreach (Tag tag in document.Tags)
            {
                AddTag(context, document, tag);
            }
        }

        private void UpdateDocumentCore(IContext context, Document document)
        {
            Audit(
                context,
                _updateDocumentSql,
                command =>
                {
                    document.ModDate = DateTime.UtcNow;

                    command.Bind("@DOC_name", document.Name.EmptyIfNull());
                    command.Bind("@DOC_comment", document.Comment.EmptyIfNull());
                    command.Bind("@DOC_modDate", document.ModDate);
                    command.Bind("@DOC_isPinned", document.IsPinnedInteger);
                    command.Bind("@DOC_ref", document.Id);

                    command.ExecuteNonQuery();
                });
        }

        /// <inheritdoc />  
        public void MoveTo(IContext context, Document document, Folder oldFolder, Folder newFolder)
        {
            Audit(
                context,
                _moveToPageSql,
                command =>
                {
                    document.ModDate = DateTime.UtcNow;

                    command.Bind("@FD_ref_old", oldFolder.Id);
                    command.Bind("@FD_ref_new", newFolder.Id);
                    command.Bind("@DOC_ref", document.Id);

                    command.ExecuteNonQuery();
                });

            Audit(
                context,
                _moveToPageFtsSql,
                command =>
                {
                    command.Bind("@FD_ref_old", oldFolder.Id.ToString());
                    command.Bind("@FD_ref_new", newFolder.Id.ToString());
                    command.Bind("@DOC_ref", document.Id.ToString());

                    command.ExecuteNonQuery();
                });

        }

        /// <inheritdoc />  
        public IEnumerable<Document> ReadRecent(IContext context, int limit)
        {
            return Audit<IEnumerable<Document>>(
                context,
                _readRecentSql,
                command =>
                {
                    command.Bind("@limit", limit);
                    return command.ExecuteQuery<Document>();
                });
        }
    }
}
