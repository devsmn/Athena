using Athena.DataModel.Core;

namespace Athena.UI
{
    /// <summary>
    /// Provides common functionality for downloading external resources.
    /// </summary>
    public interface IDownloadService : IDisposable
    {
        /// <summary>
        /// Asynchronously downloads the file at the given <paramref name="source"/> and saves
        /// it at the provided <paramref name="localFilePath"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="source"></param>
        /// <param name="localFilePath"></param>
        /// <returns></returns>
        Task DownloadAsync(IContext context, string source, string localFilePath);
    }
}
