using Athena.DataModel;
using Athena.DataModel.Core;
using SQLite;

namespace Athena.Data.SQLite
{
    internal class SQLiteTagRepository : SQLiteRepository, ITagRepository
    {
        private string readTagSql;
        private string insertTagSql;
        private string updateTagSql;
        private string deleteTagSql;

        public async Task<bool> InitializeAsync()
        {
            await RunScriptAsync("CREATE_TABLE_TAG.sql");

            readTagSql = await ReadResourceAsync("TAG_READ.sql");
            insertTagSql = await ReadResourceAsync("TAG_INSERT.sql");
            deleteTagSql = await ReadResourceAsync("TAG_DELETE.sql");
            updateTagSql = await ReadResourceAsync("TAG_UPDATE.sql");

            return true;
        }

        public IEnumerable<Tag> ReadAll(IContext context)
        {
            return Audit<IEnumerable<Tag>>(
                context,
                readTagSql,
                command =>
                {
                    command.Bind("@TAG_ref", -1);
                    return command.ExecuteQuery<Tag>();
                });
        }

        public void Save(IContext context, Tag tag)
        {
            this.Audit(context, () =>
            {
                if (tag.Key == null || tag.Id == TagKey.TemporaryId)
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
            var connection = this.Database.GetConnection();

            SQLiteCommand command = connection.CreateCommand(this.updateTagSql);

            command.Bind("@TAG_name", tag.Name.EmptyIfNull());
            command.Bind("@TAG_comment", tag.Comment.EmptyIfNull());
            command.Bind("@TAG_modDate", DateTime.UtcNow);
            command.Bind("@TAG_ref", tag.Id);
            command.ExecuteNonQuery();
        }

        private void InsertCore(IContext context, Tag tag)
        {
            var connection = this.Database.GetConnection();

            SQLiteCommand command = connection.CreateCommand(this.insertTagSql);

            command.Bind("@TAG_name", tag.Name.EmptyIfNull());
            command.Bind("@TAG_comment", tag.Comment.EmptyIfNull());
            command.Bind("@TAG_creationDate", tag.CreationDate);
            command.Bind("@TAG_modDate", tag.ModDate);
            command.ExecuteNonQuery();

            tag.SetKey(new TagKey((int)SQLite3.LastInsertRowid(connection.Handle)));
        }

        public void Delete(IContext context, Tag tag)
        {
            Audit(
                context,
                deleteTagSql,
                command =>
                {
                    command.Bind("@TAG_ref", tag.Id);
                    command.ExecuteNonQuery();
                });
        }
    }
}
