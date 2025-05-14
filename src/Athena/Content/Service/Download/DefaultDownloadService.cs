using Athena.DataModel.Core;

namespace Athena.UI
{
    public class DefaultDownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;

        public DefaultDownloadService()
        {
            _httpClient = new();
        }

        public async Task DownloadAsync(IContext context, string source, string localFilePath)
        {
            try
            {
                byte[] result = await _httpClient.GetByteArrayAsync(source);
                await File.WriteAllBytesAsync(localFilePath, result);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
