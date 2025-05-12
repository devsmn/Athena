using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;

namespace Athena.UI
{
    public interface IDownloadService : IDisposable
    {
        Task DownloadAsync(IContext context, string source, string localFilePath);
    }
}
