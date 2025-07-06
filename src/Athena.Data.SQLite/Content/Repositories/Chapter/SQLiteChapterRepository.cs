namespace Athena.Data.SQLite
{
    using DataModel;
    using DataModel.Core;

    /// <summary>
    /// Provides the sqlite specific implementation of the <see cref="IChapterRepository"/>.
    /// </summary>
    internal class SqliteChapterRepository : SqliteRepository, IChapterRepository
    {
        private string _insertChapterSql;
        private string _readChapterSql;
        private string _deleteChapterSql;

        public SqliteChapterRepository(string cipher)
            : base(cipher)
        {
        }

        public async Task<bool> InitializeAsync(IContext context)
        {
            await ValidateConnection();
            context.Log("Initializing search repository");

            _insertChapterSql = await ReadResourceAsync("CHAPTER_INSERT.sql");
            _readChapterSql = await ReadResourceAsync("CHAPTER_READ.sql");
            _deleteChapterSql = await ReadResourceAsync("CHAPTER_DELETE.sql");

            return true;
        }

        public void RegisterPatches(IContext context, ICompatibilityService compatService)
        {
            compatService.RegisterPatch<SqliteChapterRepository>(new(1, CreateTables));
        }

        private async Task CreateTables(IContext context)
        {
            context.Log("Creating search repository data storage");
            await RunScriptAsync("CREATE_TABLE_CHAPTER.sql");
        }

        public async Task ExecutePatches(IContext context, ICompatibilityService compatService)
        {
            IEnumerable<VersionPatch> patches = compatService.GetPatches<SqliteChapterRepository>();

            foreach (VersionPatch pat in patches)
            {
                await pat.PatchAsync(context);
            }
        }

        /// <inheritdoc />  
        public IEnumerable<Chapter> Search(IContext context, string pattern)
        {
            return Audit<IEnumerable<Chapter>>(
                context,
                _readChapterSql,
                command =>
                {
                    command.Bind("@CHP_text", pattern);

                    List<Chapter> chapters = command.ExecuteQuery<Chapter>();

                    foreach (Chapter chapter in chapters)
                    {
                        chapter.ReadDocument(context);
                        chapter.ReadFolder(context);
                    }

                    return chapters;
                });
        }


        /// <inheritdoc />  
        public void Delete(IContext context, int documentRef)
        {
            Audit(
                context,
                _deleteChapterSql,
                command =>
                {
                    command.Bind("@DOC_ref", documentRef.ToString());
                    command.ExecuteNonQuery();
                });
        }

        /// <inheritdoc />  
        public void Save(IContext context, Chapter chapter)
        {
            Audit(
                context,
                _insertChapterSql,
                command =>
                {
                    command.Bind("@DOC_ref", chapter.DocumentId);
                    command.Bind("@DOC_pageNr", chapter.DocumentPageNumber);
                    command.Bind("@CHP_text", chapter.Snippet);
                    command.Bind("@FD_ref", chapter.FolderId);

                    command.ExecuteNonQuery();
                });
        }
    }
}
