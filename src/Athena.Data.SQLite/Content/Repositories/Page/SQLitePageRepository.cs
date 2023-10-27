
using Athena.DataModel.Core;
using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;

namespace Athena.Data.SQLite
{
    using Athena.DataModel;

    /// <summary>
    /// Provides the SQLite implementation of <see cref="IPageRepository"/>.
    /// </summary>
    internal class SQLitePageRepository : SQLiteRepository, IPageRepository
    {
        private string pageInsertSql;
        private string pageReadSql;
        private string insertPageDocumentSql;
        private string pageDeleteSql;

        public void AddTag(Tag tag)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> InitializeAsync()
        {
            await this.RunScriptAsync("CREATE_TABLE_PAGE.sql");
            await this.RunScriptAsync("CREATE_TABLE_PAGE_DOC.sql");
            await this.RunScriptAsync("CREATE_TABLE_PAGE_TAG.sql");

            pageInsertSql = await ReadResouceAsync("PAGE_INSERT.sql");
            pageReadSql = await ReadResouceAsync("PAGE_READ.sql");
            insertPageDocumentSql = await ReadResouceAsync("PAGE_DOC_INSERT.sql");
            pageDeleteSql = await ReadResouceAsync("PAGE_DELETE.sql");
            
            return await Task.FromResult(true);
        }


        public IEnumerable<Page> ReadAll(IContext context)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.pageReadSql);

                command.Bind("@PG_ref", -1);

                var pages = command.ExecuteQuery<Page>();

                foreach (var page in pages)
                {
                    page.ReadAllDocuments(context);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return Enumerable.Empty<Page>();
        }

        public Page Read(IContext context, Page page)
        {
            throw new NotImplementedException();
        }
        
        public void Delete(IContext context, Page page)
        {
            var connection = this.Database.GetConnection();

            SQLiteCommand command = connection.CreateCommand(this.pageDeleteSql);

            command.Bind("@PG_ref", page.Key.Id);
            command.ExecuteNonQuery();
        }

        public void Save(IContext context, Page page)
        {
            try
            {
                if (page.Key == null || page.Key.Id == PageKey.TemporaryId)
                {
                    InsertPageCore(context, page);
                }
                else
                {
                    UpdatePageCore(context, page);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }

        private void InsertPageCore(IContext context, Page page)
        {
            try
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.pageInsertSql);

                command.Bind("@PG_title", page.Title.EmptyIfNull());
                command.Bind("@PG_comment", page.Comment.EmptyIfNull());
                command.Bind("@PG_modDate", page.ModDate);
                command.Bind("@PG_creationDate", page.CreationDate);
                command.ExecuteNonQuery();

                page.SetKey(new PageKey((int)SQLite3.LastInsertRowid(connection.Handle)));

                foreach (var document in page.Documents)
                {
                    AddDocument(context, page, document);
                }
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }
        }

        private void UpdatePageCore(IContext context, Page page)
        {
            foreach (var document in page.Documents)
            {
                AddDocument(context, page, document);
            }
        }


        public void AddDocument(IContext context, Page page, Document document)
        {
            if (document.Key != null && document.Key.Id != DocumentKey.TemporaryId)
                return;

            document.Save(context);

            var connection = this.Database.GetConnection();

            var command = connection.CreateCommand(this.insertPageDocumentSql);

            command.Bind("@PG_ref", page.Id);
            command.Bind("@DOC_ref", document.Id);
            command.Bind("@PGDOC_creationDate", DateTime.UtcNow);
            command.Bind("@PGDOC_modDate", DateTime.UtcNow);

            command.ExecuteNonQuery();
        }

    }
}
