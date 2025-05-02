namespace Athena.DataModel.Core
{
    public interface ICompressionService
    {
        /// <summary>
        /// Asynchronously compresses the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Task<byte[]> CompressAsync(MemoryStream stream);

        /// <summary>
        /// Compresses the given <paramref name="data"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] data);

        public Task<byte[]> DecompressAsync(MemoryStream stream);
        public byte[] Decompress(byte[] data);
    }
}
