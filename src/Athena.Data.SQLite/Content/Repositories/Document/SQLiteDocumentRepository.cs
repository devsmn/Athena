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
    internal class SQLiteDocumentRepository : SQLiteRepository, IDocumentRepository
    {
        private string insertDocumentSql;
        private string insertDocumentTagSql;
        private string readDocumentSql;
        private string readPageDocumentSql;
        private string readDocumentTagSql;
        private string deleteDocumentSql;
        private string deleteTagSql;
        private string searchDocWithTagSql;
        private string searchDocNoTagSql;
        private string readDocumentPdfSql;
        private string updateDocumentSql;

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_DOCUMENT.sql");
            await RunScriptAsync("CREATE_TABLE_DOCUMENT_TAG.sql");

            insertDocumentSql = await ReadResourceAsync("DOCUMENT_INSERT.sql");
            insertDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_INSERT.sql");
            readDocumentSql = await ReadResourceAsync("DOCUMENT_READ.sql");
            readPageDocumentSql = await ReadResourceAsync("PAGE_DOC_READ.sql");
            deleteDocumentSql = await ReadResourceAsync("DOCUMENT_DELETE.sql");
            readDocumentTagSql = await ReadResourceAsync("DOCUMENT_TAG_READ.sql");
            deleteTagSql = await ReadResourceAsync("DOCUMENT_TAG_DELETE.sql");
            searchDocWithTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_TAG.sql"); ;
            searchDocNoTagSql = await ReadResourceAsync("DOCUMENT_SEARCH_NOTAG.sql");
            readDocumentPdfSql = await ReadResourceAsync("DOCUMENT_READ_PDF.sql");
            updateDocumentSql = await ReadResourceAsync("DOCUMENT_UPDATE.sql"); // 

            return await Task.FromResult(true);
        }

        public IEnumerable<Document> ReadAll(IContext context, Page page)
        {
            return Audit<IEnumerable<Document>>(
                context,
                readPageDocumentSql,
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
                readDocumentSql,
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
                deleteTagSql,
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
                insertDocumentTagSql,
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
                readDocumentTagSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    return command.ExecuteQuery<Tag>();
                });
        }

        public IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFTS)
        {
            documentName = string.IsNullOrEmpty(documentName) ? string.Empty : $"%{documentName}%";

            return this.Audit(context, () =>
            {
                IEnumerable<SearchResult> results = new List<SearchResult>();

                if (tags.Any())
                {
                    results = SearchWithTags(documentName, tags, useFTS);
                }
                else
                {
                    results = SearchNoTags(documentName, useFTS);
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
                readDocumentPdfSql,
                command =>
                {
                    command.Bind("@DOC_ref", document.Id);
                    return command.ExecuteScalar<string>();
                });
        }

        private IEnumerable<SearchResult> SearchNoTags(string documentName, bool useFTS)
        {
            var connection = this.Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(this.searchDocNoTagSql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFTS ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        private IEnumerable<SearchResult> SearchWithTags(string documentName, IEnumerable<Tag> tags, bool useFTS)
        {
            string ids = string.Join(",", tags.Select(x => x.Id));

            string sql = searchDocWithTagSql.Replace("<<__replace__>>", ids);

            var connection = this.Database.GetConnection();
            SQLiteCommand command = connection.CreateCommand(sql);

            command.Bind("@DOC_name", documentName);
            command.Bind("@useFTS", useFTS ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        public void Delete(IContext context, Document document)
        {
            Audit(
                context,
                deleteDocumentSql,
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
            SQLiteCommand command = connection.CreateCommand(this.insertDocumentSql);

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
                updateDocumentSql,
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
