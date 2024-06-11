namespace Athena.Data.SQLite
{
    using Athena.DataModel;
    using Athena.DataModel.Core;

    internal class SqLiteChapterRepository : SqLiteRepository, IChapterRepository
    {
        private string _insertChapterSql;
        private string _readChapterSql;
        private string _deleteChapterSql;

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_CHAPTER.sql");

            _insertChapterSql = await ReadResourceAsync("CHAPTER_INSERT.sql");
            _readChapterSql = await ReadResourceAsync("CHAPTER_READ.sql");
            _deleteChapterSql = await ReadResourceAsync("CHAPTER_DELETE.sql");

            return true;
        }

        public IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            return Audit<IEnumerable<Chapter>>(
                context,
                _readChapterSql,
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
                _deleteChapterSql,
                command => {
                    command.Bind("@DOC_ref", documentRef.ToString());
                    command.ExecuteNonQuery();
                });
        }

        public void Save(IContext context, Chapter chapter)
        {
            Audit(
                context,
                _insertChapterSql,
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
