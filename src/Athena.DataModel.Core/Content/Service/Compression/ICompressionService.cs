using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCompressor;

namespace Athena.DataModel.Core
{
    public interface ICompressionService
    {
        public Task<byte[]> CompressAsync(MemoryStream stream);
        public Task<byte[]> Decompress(MemoryStream stream);
    }
}
