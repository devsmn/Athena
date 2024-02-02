namespace Athena.DataModel.Core
{
    public class IntegerEntityKey : EntityKey<int>
    {
        public static int TemporaryId { get; } = -1;

        public IntegerEntityKey(int key)
            : base(key)
        {
        }
    }
}
