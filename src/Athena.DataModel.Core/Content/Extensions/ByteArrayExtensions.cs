using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public static class ByteArrayExtensions
    {
        public static byte[] With(this byte[] first, params byte[][] other)
        {
            int totalLength = first.Length + other.Sum(x => x.Length);
            byte[] combined = new byte[totalLength];

            Buffer.BlockCopy(first, 0, combined, 0, first.Length);
            int offset = first.Length;

            foreach (var array in other)
            {
                Buffer.BlockCopy(array, 0, combined, offset, array.Length);
                offset += array.Length;
            }

            return combined;
        }

        public static bool ConstantTimeIsEqualTo(this byte[] source, byte[] to)
        {
            if (source == null || to == null || source.Length != to.Length)
                return false;

            int result = 0;
            for (int i = 0; i < source.Length; i++)
            {
                result |= source[i] ^ to[i];
            }
            return result == 0;
        }
    }
}
