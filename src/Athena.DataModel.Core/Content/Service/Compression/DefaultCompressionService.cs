using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCompressor;

namespace Athena.DataModel.Core
{
    public class DefaultCompressionService : ICompressionService
    {
        private readonly ICompressor _compressor;

        public DefaultCompressionService()
        {
            _compressor = new LZ4Compressor();
        }

        public async Task<byte[]> CompressAsync(MemoryStream stream)
        {
            return await _compressor.CompressAsync(stream);
        }

        public async Task<byte[]> Decompress(MemoryStream stream)
        {
            return await _compressor.DecompressAsync(stream);
        }
    }
}
