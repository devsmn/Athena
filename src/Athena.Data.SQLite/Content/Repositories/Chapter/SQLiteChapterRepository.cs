namespace Athena.Data.SQLite
{
    using Athena.DataModel;
    using Athena.DataModel.Core;

    internal class SQLiteChapterRepository : SQLiteRepository, IChapterRepository
    {
        private string insertChapterSql;
        private string readChapterSql;
        private string deleteChapterSql;

        public async Task<bool> InitializeAsync()
        {
            // TODO: Move to central
            await this.RunScriptAsync("CREATE_TABLE_CHAPTER.sql");

            insertChapterSql = await ReadResouceAsync("CHAPTER_INSERT.sql");
            readChapterSql = await ReadResouceAsync("CHAPTER_READ.sql");
            deleteChapterSql = await ReadResouceAsync("CHAPTER_DELETE.sql");

            return true;
        }

        public IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            try
            {
                var connection = Database.GetConnection();

                var command = connection.CreateCommand(this.readChapterSql);

                command.Bind("@CHP_text", pattern);

                var chapters = command.ExecuteQuery<Chapter>();

                foreach (var chapter in chapters)
                {
                    chapter.ReadDocument(context);
                    chapter.ReadPage(context);
                    chapter.ReadFolder(context);
                }

                return chapters;
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return Enumerable.Empty<Chapter>();
        }

        public void Delete(IContext context, int documentRef)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(deleteChapterSql);

                command.Bind("@DOC_ref", documentRef.ToString());
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        public void Save(IContext context, Chapter chapter)
        {
            try
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(insertChapterSql);

                command.Bind("@DOC_ref", chapter.DocumentId);
                command.Bind("@DOC_pageNr", chapter.DocumentPageNumber);
                command.Bind("@CHP_text", chapter.Snippet);
                command.Bind("@FD_ref", chapter.FolderId);
                command.Bind("@PG_ref", chapter.PageId);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }
    }
}
