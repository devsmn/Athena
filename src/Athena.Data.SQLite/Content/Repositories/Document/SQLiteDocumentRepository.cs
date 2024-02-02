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
            await this.RunScriptAsync("CREATE_TABLE_DOCUMENT.sql");
            await this.RunScriptAsync("CREATE_TABLE_DOCUMENT_TAG.sql");

            insertDocumentSql = await ReadResouceAsync("DOCUMENT_INSERT.sql");
            insertDocumentTagSql = await ReadResouceAsync("DOCUMENT_TAG_INSERT.sql");
            readDocumentSql = await ReadResouceAsync("DOCUMENT_READ.sql");
            readPageDocumentSql = await ReadResouceAsync("PAGE_DOC_READ.sql");
            deleteDocumentSql = await ReadResouceAsync("DOCUMENT_DELETE.sql");
            readDocumentTagSql = await ReadResouceAsync("DOCUMENT_TAG_READ.sql");
            deleteTagSql = await ReadResouceAsync("DOCUMENT_TAG_DELETE.sql");
            searchDocWithTagSql = await ReadResouceAsync("DOCUMENT_SEARCH_TAG.sql");;
            searchDocNoTagSql = await ReadResouceAsync("DOCUMENT_SEARCH_NOTAG.sql");
            readDocumentPdfSql = await ReadResouceAsync("DOCUMENT_READ_PDF.sql");
            updateDocumentSql = await ReadResouceAsync("DOCUMENT_UPDATE.sql"); // 

            return await Task.FromResult(true);
        }

        public IEnumerable<Document> ReadAll(IContext context, Page page)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readPageDocumentSql);
                command.Bind("@PG_ref", page.Id);

                var document =  command.ExecuteQuery<Document>();
                
                return document;
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Document>();
        }

        public Document Read(IContext context, DocumentKey key)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readDocumentSql);
                command.Bind("@DOC_ref", key.Id);

                return command.ExecuteQuery<Document>()[0];
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return new Document();
        }

        public void DeleteTag(IContext context, Document document, Tag tag)
        {
            this.Audit(context, () => {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.deleteTagSql);
                command.Bind("@DOC_ref", document.Id);
                command.Bind("@TAG_ref", tag.Id);

                command.ExecuteNonQuery();
            });
        }

        public void AddTag(IContext context, Document document, Tag tag)
        {
            Audit(context, () => {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.insertDocumentTagSql);
                command.Bind("@DOC_ref", document.Id);
                command.Bind("@TAG_ref", tag.Id);
                command.Bind("@DOCTAG_creationDate", DateTime.UtcNow);
                command.Bind("@DOCTAG_modDate", DateTime.MinValue);

                command.ExecuteNonQuery();
                
            });
        }

        public IEnumerable<Tag> ReadTags(IContext context, Document document)
        {
            return Audit<IEnumerable<Tag>>(context, () => {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readDocumentTagSql);
                command.Bind("@DOC_ref", document.Id);

                return command.ExecuteQuery<Tag>();
            });
        }

        public IEnumerable<SearchResult> Search(IContext context, string documentName, IEnumerable<Tag> tags, bool useFTS)
        {
            documentName = string.IsNullOrEmpty(documentName) ? string.Empty : $"%{documentName}%";

            return this.Audit(context, () => {
                IEnumerable<SearchResult> results = new List<SearchResult>();
                
                if (tags.Any())
                {
                    results = SearchWithTags(documentName, tags, useFTS);
                }
                else
                {
                    results =  SearchNoTags(documentName, useFTS);
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
            return this.Audit<string>(context, () => {
                var connection = this.Database.GetConnection();
                SQLiteCommand command = connection.CreateCommand(this.readDocumentPdfSql);

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
            
            command.Bind("@DOC_name",  documentName);
            command.Bind("@useFTS", useFTS ? 1 : 0);

            return command.ExecuteQuery<SearchResult>();
        }

        public void Delete(IContext context, Document document)
        {
            try
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.deleteDocumentSql);

                Debug.Assert(document.Key.Id != DocumentKey.TemporaryId);

                command.Bind("@DOC_ref", document.Key.Id);
                command.ExecuteNonQuery();

                Chapter.Delete(context, document.Key.Id);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
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
            this.Audit(context, () => {
                var connection = this.Database.GetConnection();
                SQLiteCommand command = connection.CreateCommand(this.updateDocumentSql);

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
