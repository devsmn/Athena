namespace Athena.DataModel.Core
{
    public class IntegerEntityKey
    {
        private static readonly Lazy<IntegerEntityKey> key = new Lazy<IntegerEntityKey>(() => new IntegerEntityKey(0));

        public static IntegerEntityKey Root => key.Value;

        public static int TemporaryId { get; } = -1;
        public int Id { get; set; }

        public IntegerEntityKey(int key)
        {
            Id = key;
        }
    }
}
