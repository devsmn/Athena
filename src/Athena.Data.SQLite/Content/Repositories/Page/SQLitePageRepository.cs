
using Athena.DataModel.Core;
using SQLite;

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
        private string pageUpdateSql;

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
            pageUpdateSql = await ReadResouceAsync("PAGE_UPDATE.sql");

            return await Task.FromResult(true);
        }


        public IEnumerable<Page> ReadAll(IContext context)
        {
            return this.Audit<IEnumerable<Page>>(context, () =>
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.pageReadSql);

                command.Bind("@PG_ref", -1);

                var pages = command.ExecuteQuery<Page>();
                
                return pages;
            });
        }
        
        public Page Read(IContext context, PageKey key)
        {
            return this.Audit<Page>(context, () => {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.pageReadSql);

                command.Bind("@PG_ref", key.Id);

                return command.ExecuteQuery<Page>()[0];
            });
        }
        
        public void Delete(IContext context, Page page)
        {
            this.Audit(context, () =>
            {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.pageDeleteSql);

                command.Bind("@PG_ref", page.Key.Id);
                command.ExecuteNonQuery();

                foreach (var document in page.Documents)
                {
                    document.Delete(context);
                }
            });
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
            this.Audit(context, () => {
                page.ModDate = DateTime.Now;

                var connection = this.Database.GetConnection();
                var command = connection.CreateCommand(this.pageUpdateSql);

                command.Bind("@PG_title", page.Title.EmptyIfNull());
                command.Bind("@PG_comment", page.Comment.EmptyIfNull());
                command.Bind("@PG_modDate", page.ModDate);
                command.Bind("@PG_ref", page.Id);

                command.ExecuteNonQuery();

                foreach (var document in page.Documents)
                {
                    AddDocument(context, page, document);
                }
            });
        }


        public void AddDocument(IContext context, Page page, Document document)
        {
            if (document.Key != null && document.Key.Id != DocumentKey.TemporaryId)
                return;

            this.Audit(context, () => {
                document.Save(context);

                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.insertPageDocumentSql);

                command.Bind("@PG_ref", page.Id);
                command.Bind("@DOC_ref", document.Id);
                command.Bind("@PGDOC_creationDate", DateTime.UtcNow);
                command.Bind("@PGDOC_modDate", DateTime.UtcNow);

                command.ExecuteNonQuery();
            });
        }

    }
}
