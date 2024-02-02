using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Tag : Entity<TagKey>
    {
        public int Id
        {
            get { return Key.Id; }
            set { Key.Id = value; }
        }

        public string Name { get; set; }
        public string Comment { get; set; }

        public Tag(TagKey key) : base(key)
        {
        }

        public Tag()
            : this(new TagKey(TagKey.TemporaryId))
        {
        }
    }
}
