using Athena.DataModel;
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    internal class SqliteTagRepository : SqliteRepository, ITagRepository
    {
        private string _readTagSql;
        private string _insertTagSql;
        private string _updateTagSql;
        private string _deleteTagSql;

        public SqliteTagRepository(string cipher) : base(cipher)
        {
        }

        public async Task<bool> InitializeAsync(IContext context)
        {
            await ValidateConnection();
            context.Log("Initializing tag repository");

            _readTagSql = await ReadResourceAsync("TAG_READ.sql");
            _insertTagSql = await ReadResourceAsync("TAG_INSERT.sql");
            _deleteTagSql = await ReadResourceAsync("TAG_DELETE.sql");
            _updateTagSql = await ReadResourceAsync("TAG_UPDATE.sql");

            return true;
        }

        public void RegisterPatches(IContext context, ICompatibilityService compatService)
        {
            compatService.RegisterPatch<SqliteTagRepository>(new(1, CreateTables));
        }

        private async Task CreateTables(IContext context)
        {
            context.Log("Creating tag data storage");
            await RunScriptAsync("CREATE_TABLE_TAG.sql");
        }

        public async Task ExecutePatches(IContext context, ICompatibilityService compatService)
        {
            var patches = compatService.GetPatches<SqliteTagRepository>();

            foreach (var pat in patches)
            {
                await pat.PatchAsync(context);
            }
        }

        public IEnumerable<Tag> ReadAll(IContext context)
        {
            return Audit<IEnumerable<Tag>>(
                context,
                _readTagSql,
                command =>
                {
                    command.Bind("@TAG_ref", -1);
                    return command.ExecuteQuery<Tag>();
                });
        }

        [Obsolete]
        public void Save(IContext context, Tag tag)
        {
            Audit(context, () =>
            {
                if (tag.Key == null || tag.Id == IntegerEntityKey.TemporaryId)
                {
                    InsertCore(context, tag);
                }
                else
                {
                    UpdateCore(context, tag);
                }
            });
        }

        private void UpdateCore(IContext context, Tag tag)
        {
            var connection = Database.GetConnection();

            SQLiteCommand command = connection.CreateCommand(_updateTagSql);

            command.Bind("@TAG_name", tag.Name.EmptyIfNull());
            command.Bind("@TAG_comment", tag.Comment.EmptyIfNull());
            command.Bind("@TAG_modDate", DateTime.UtcNow);
            command.Bind("@TAG_backgroundColor", tag.BackgroundColor.EmptyIfNull());
            command.Bind("@TAG_textColor", tag.TextColor.EmptyIfNull());
            command.Bind("@TAG_ref", tag.Id);
            command.ExecuteNonQuery();
        }

        private void InsertCore(IContext context, Tag tag)
        {
            var connection = Database.GetConnection();

            SQLiteCommand command = connection.CreateCommand(_insertTagSql);

            command.Bind("@TAG_name", tag.Name.EmptyIfNull());
            command.Bind("@TAG_comment", tag.Comment.EmptyIfNull());
            command.Bind("@TAG_backgroundColor", tag.BackgroundColor.EmptyIfNull());
            command.Bind("@TAG_textColor", tag.TextColor.EmptyIfNull());
            command.Bind("@TAG_creationDate", tag.CreationDate);
            command.Bind("@TAG_modDate", tag.ModDate);
            command.ExecuteNonQuery();

            tag.Key = new IntegerEntityKey((int)SQLite3.LastInsertRowid(connection.Handle));
        }

        public void Delete(IContext context, Tag tag)
        {
            Audit(
                context,
                _deleteTagSql,
                command =>
                {
                    command.Bind("@TAG_ref", tag.Id);
                    command.ExecuteNonQuery();
                });
        }
    }
}
