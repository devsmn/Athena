
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    using Athena.DataModel;

    /// <summary>
    /// Provides the SQLite implementation of <see cref="IPageRepository"/>.
    /// </summary>
    internal class SqLitePageRepository : SqLiteRepository, IPageRepository
    {
        private string _pageInsertSql;
        private string _pageReadSql;
        private string _insertPageDocumentSql;
        private string _pageDeleteSql;
        private string _pageUpdateSql;

        public void AddTag(Tag tag)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_PAGE.sql");
            await RunScriptAsync("CREATE_TABLE_PAGE_DOC.sql");
            await RunScriptAsync("CREATE_TABLE_PAGE_TAG.sql");

            _pageInsertSql = await ReadResourceAsync("PAGE_INSERT.sql");
            _pageReadSql = await ReadResourceAsync("PAGE_READ.sql");
            _insertPageDocumentSql = await ReadResourceAsync("PAGE_DOC_INSERT.sql");
            _pageDeleteSql = await ReadResourceAsync("PAGE_DELETE.sql");
            _pageUpdateSql = await ReadResourceAsync("PAGE_UPDATE.sql");

            return await Task.FromResult(true);
        }

        public IEnumerable<Page> ReadAll(IContext context)
        {
            return Audit<IEnumerable<Page>>(
                context,
                _pageReadSql,
                command =>
                {
                    command.Bind("@PG_ref", -1);
                    return command.ExecuteQuery<Page>();
                });
        }

        public Page Read(IContext context, PageKey key)
        {
            return Audit(
                context,
                _pageReadSql,
                command =>
                {
                    command.Bind("@PG_ref", key.Id);
                    return command.ExecuteQuery<Page>()[0];
                });
        }

        public void Delete(IContext context, Page page)
        {
            Audit(
                context,
                _pageDeleteSql,
                command =>
                {
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

                SQLiteCommand command = connection.CreateCommand(this._pageInsertSql);

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
            Audit(
                context,
                _pageUpdateSql,
                command =>
                {
                    page.ModDate = DateTime.Now;

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

            Audit(
                context,
                _insertPageDocumentSql,
                command =>
                {
                    document.Save(context);

                    command.Bind("@PG_ref", page.Id);
                    command.Bind("@DOC_ref", document.Id);
                    command.Bind("@PGDOC_creationDate", DateTime.UtcNow);
                    command.Bind("@PGDOC_modDate", DateTime.UtcNow);

                    command.ExecuteNonQuery();
                });
        }

    }
}
