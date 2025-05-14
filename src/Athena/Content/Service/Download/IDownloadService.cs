using Athena.DataModel.Core;

namespace Athena.UI
{
    public interface IDownloadService : IDisposable
    {
        Task DownloadAsync(IContext context, string source, string localFilePath);
    }
}
