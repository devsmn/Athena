using Athena.DataModel;
using Athena.DataModel.Core;
using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Page = Athena.DataModel.Page;

namespace Athena.Data.SQLite
{
    /// <summary>
    /// Provides the SQLite implementation of <see cref="IDocumentRepository"/>.
    /// </summary>
    internal class SQLiteDocumentRepository : SQLiteRepository, IDocumentRepository
    {
        private string insertDocumentSql;
        private string readDocumentSql;
        private string readPageDocumentSql;
        private string deleteDocumentSql;

        public async Task<bool> InitializeAsync()
        {
            // TODO: Move to central
            await this.RunScriptAsync("CREATE_TABLE_DOCUMENT.sql");
            await this.RunScriptAsync("CREATE_TABLE_TAG.sql");

            insertDocumentSql = await ReadResouceAsync("DOCUMENT_INSERT.sql");
            readDocumentSql = await ReadResouceAsync("DOCUMENT_READ.sql");
            readPageDocumentSql = await ReadResouceAsync("PAGE_DOC_READ.sql");
            deleteDocumentSql = await ReadResouceAsync("DOCUMENT_DELETE.sql");

            return await Task.FromResult(true);
        }

        public IEnumerable<Document> ReadAll(IContext context, Page page)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readPageDocumentSql);
                command.Bind("@PG_ref", page.Id);

                return command.ExecuteQuery<Document>();
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Document>();
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
                    SaveDocumentCore(context, document);
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
            command.Bind("@DOC_image", Convert.ToBase64String(document.Image ?? Array.Empty<byte>()));
            command.Bind("@DOC_creationDate", document.CreationDate);
            command.Bind("@DOC_modDate", document.ModDate);

            command.ExecuteNonQuery();

            document.SetKey(new DocumentKey((int)SQLite3.LastInsertRowid(connection.Handle)));
        }

        private void SaveDocumentCore(IContext context, Document document)
        {
        }
    }
}
