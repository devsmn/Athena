using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Tag : Entity
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }

        public Tag(IntegerEntityKey key) : base(key)
        {
        }

        public Tag()
            : this(new IntegerEntityKey(IntegerEntityKey.TemporaryId))
        {
        }
    }
}
