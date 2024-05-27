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
            await RunScriptAsync("CREATE_TABLE_CHAPTER.sql");

            insertChapterSql = await ReadResourceAsync("CHAPTER_INSERT.sql");
            readChapterSql = await ReadResourceAsync("CHAPTER_READ.sql");
            deleteChapterSql = await ReadResourceAsync("CHAPTER_DELETE.sql");

            return true;
        }

        public IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            return Audit<IEnumerable<Chapter>>(
                context,
                readChapterSql,
                command => {
                    command.Bind("@CHP_text", pattern);

                    var chapters = command.ExecuteQuery<Chapter>();

                    foreach (var chapter in chapters)
                    {
                        chapter.ReadDocument(context);
                        chapter.ReadPage(context);
                        chapter.ReadFolder(context);
                    }

                    return chapters;
                });
        }

        public void Delete(IContext context, int documentRef)
        {
            Audit(
                context,
                deleteChapterSql,
                command => {
                    command.Bind("@DOC_ref", documentRef.ToString());
                    command.ExecuteNonQuery();
                });
        }

        public void Save(IContext context, Chapter chapter)
        {
            Audit(
                context,
                insertChapterSql,
                command => {
                    command.Bind("@DOC_ref", chapter.DocumentId);
                    command.Bind("@DOC_pageNr", chapter.DocumentPageNumber);
                    command.Bind("@CHP_text", chapter.Snippet);
                    command.Bind("@FD_ref", chapter.FolderId);
                    command.Bind("@PG_ref", chapter.PageId);

                    command.ExecuteNonQuery();
                });
        }
    }
}
