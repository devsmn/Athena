namespace Athena.DataModel.Core
{
    public class Entity
    {
        public IntegerEntityKey Key { get; set; }

        public int Id
        {
            get => Key.Id;
            set => Key = new IntegerEntityKey(value);
        }

        public DateTime CreationDate
        {
            get; set;
        }

        public DateTime ModDate
        {
            get; set;
        }

        public Entity(IntegerEntityKey key)
        {
            CreationDate = DateTime.UtcNow;
            ModDate = DateTime.UtcNow;
            Key = key;
        }
    }
}
