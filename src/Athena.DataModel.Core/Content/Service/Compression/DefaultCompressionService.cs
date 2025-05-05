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

        public byte[] Compress(byte[] data)
        {
            return _compressor.Compress(data);
        }

        public async Task<byte[]> DecompressAsync(MemoryStream stream)
        {
            return await _compressor.DecompressAsync(stream);
        }

        public byte[] Decompress(byte[] data)
        {
            return _compressor.Decompress(data);
        }
    }
}
