namespace Athena.DataModel.Core
{
    public static class ByteArrayExtensions
    {
        public static bool IsNullOrEmpty(this byte[] array)
        {
            return array == null || array.Length == 0;
        }

        public static byte[] With(this byte[] first, params byte[][] other)
        {
            int totalLength = first.Length + other.Sum(x => x.Length);
            byte[] combined = new byte[totalLength];

            Buffer.BlockCopy(first, 0, combined, 0, first.Length);
            int offset = first.Length;

            foreach (byte[] array in other)
            {
                Buffer.BlockCopy(array, 0, combined, offset, array.Length);
                offset += array.Length;
            }

            return combined;
        }
    }
}
