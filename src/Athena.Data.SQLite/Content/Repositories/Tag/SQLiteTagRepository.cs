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
            await this.RunScriptAsync("CREATE_TABLE_TAG.sql");

            readTagSql = await ReadResouceAsync("TAG_READ.sql");
            insertTagSql = await ReadResouceAsync("TAG_INSERT.sql");
            deleteTagSql = await ReadResouceAsync("TAG_DELETE.sql");
            updateTagSql = await ReadResouceAsync("TAG_UPDATE.sql");

            return true;
        }

        public IEnumerable<Tag> ReadAll(IContext context)
        {
            return this.Audit<IEnumerable<Tag>>(context, () =>
            {
                var connection = this.Database.GetConnection();

                var command = connection.CreateCommand(this.readTagSql);
                command.Bind("@TAG_ref", -1);

                var tags = command.ExecuteQuery<Tag>();
                return tags;
            });
        }

        public void Save(IContext context, Tag tag)
        {
            this.Audit(context, () => {
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
            this.Audit(context, () => {
                var connection = this.Database.GetConnection();

                SQLiteCommand command = connection.CreateCommand(this.deleteTagSql);

                command.Bind("@TAG_ref", tag.Id);
                command.ExecuteNonQuery();
            });
        }
    }
}
