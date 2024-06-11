using Athena.DataModel;
using Athena.DataModel.Core;
using SQLite;
using System.Diagnostics;
using Page = Athena.DataModel.Page;

namespace Athena.Data.SQLite
{
    /// <summary>
    /// Provides the SQLite implementation of <see cref="IDocumentRepository"/>.
    /// </summary>
    internal class SqLiteDocumentRepository : SqLiteRepository, IDocumentRepository
    {
        private string _insertDocumentSql;
        private string _insertDocumentTagSql;
        private string _readDocumentSql;
        private string _readPageDocumentSql;
        private string _readDocumentTagSql;
        private string _deleteDocumentSql;
        private string _deleteTagSql;
        private string _searchDocWithTagSql;
        private string _searchDocNoTagSql;
        private string _readDocumentPdfSql;
        private string _updateDocumentSql;

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_DOCUMENT.sql");
            await RunScriptAsync("CREATE_TABLE_DOCUMENT_TAG.sql");

            _insertDocumentSql = await ReadResourceAsync("DOCUMENT_INSERT.sql");
            _insertDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_INSERT.sql");
            _readDocumentSql = await ReadResourceAsync("DOCUMENT_READ.sql");
            _readPageDocumentSql = await ReadResourceAsync("PAGE_DOC_READ.sql");
            _deleteDocumentSql = await ReadResourceAsync("DOCUMENT_DELETE.sql");
            _readDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_READ.sql");
            _deleteTagSql = await ReadResourceAsync("DOCUMENT_TAG_DELETE.sql");
            _searchDocWithTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_TAG.sql"); ;
            _searchDocNoTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_NOTAG.sql");
            _readDocumentPdfSql = await ReadResourceAsync("DOCUMENT_READ_PDF.sql");
            _updateDocumentSql = await ReadResourceAsync("DOCUMENT_UPDATE.sql"); // 

            return await Task.FromResult(true);
        }

        public IEnumerable<Document> ReadAll(IContext context, Page page)
        {
            return Audit<IEnumerable<Document>>(
                context,
                _readPageDocumentSql,
                command =>
                {
                    command.Bind("@PG_ref", page.Id);
                    var document = command.ExecuteQuery<Document>();

                    return document;
                });
        }

        public Document Read(IContext context, DocumentKey key)
        {
            return Audit(
                context,
                _readDocumentSql,
                command =>
                {
                    command.Bind("@DOC_ref", key.Id);
                    return command.ExecuteQuery<Document>()[0];
                });
        }

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

        public IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFts)
        {
            documentName = string.IsNullOrEmpty(documentName) ? string.Empty : $"%{documentName}%";

            return this.Audit(context, () =>
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

                foreach (var result in results)
                {
                    result.Fill();
                }

                return results;
            });
        }

        public string ReadPdfAsString(IContext context, Document document)
        {
            return Audit(
                context,
                _readDocumentPdfSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    return command.ExecuteScalar<string>();
                });
        }

        private IEnumerable<SearchResult> SearchNoTags(string documentName, bool useFts)
        {
            var connection = this.Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(this._searchDocNoTagSql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFts ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        private IEnumerable<SearchResult> SearchWithTags(string documentName, IEnumerable<Tag> tags, bool useFts)
        {
            string ids = string.Join(",", tags.Select(x => x.Id));

            string sql = _searchDocWithTagSql.Replace("<<__replace__>>", ids);

            var connection = this.Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(sql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFts ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        public void Delete(IContext context, Document document)
        {
            Audit(
                context,
                _deleteDocumentSql,
                command =>
                {
                    Debug.Assert(document.Key.Id != DocumentKey.TemporaryId);

                    command.Bind("@DOC_ref", document.Key.Id);
                    command.ExecuteNonQuery();

                    Chapter.Delete(context, document.Key.Id);
                });
        }

        public void Save(IContext context, Document document)
        {
            try
            {
                if (document.Key == null || document.Id == DocumentKey.TemporaryId)
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
            var connection = this.Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(this._insertDocumentSql);

            command.Bind("@DOC_name", document.Name.EmptyIfNull());
            command.Bind("@DOC_comment", document.Comment.EmptyIfNull());
            command.Bind("@DOC_pdf", Convert.ToBase64String(document.Pdf ?? Array.Empty<byte>()));
            command.Bind("@DOC_thumbnail", Convert.ToBase64String(document.Thumbnail ?? Array.Empty<byte>()));
            command.Bind("@DOC_creationDate", DateTime.UtcNow);
            command.Bind("@DOC_modDate", DateTime.UtcNow);

            command.ExecuteNonQuery();

            document.SetKey(new DocumentKey((int)SQLite3.LastInsertRowid(connection.Handle)));

            foreach (var tag in document.Tags)
            {
                AddTag(context, document, tag);
            }
        }

        private void UpdateDocumentCore(IContext context, Document document)
        {
            Audit(
                context,
                _updateDocumentSql,
                command => {
                    document.ModDate = DateTime.UtcNow;

                    command.Bind("@DOC_name", document.Name.EmptyIfNull());
                    command.Bind("@DOC_comment", document.Comment.EmptyIfNull());
                    command.Bind("@DOC_modDate", document.ModDate);
                    command.Bind("@DOC_ref", document.Id);

                    command.ExecuteNonQuery();
                });
        }
    }
}
