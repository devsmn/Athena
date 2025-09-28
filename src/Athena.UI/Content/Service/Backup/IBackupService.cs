using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
using Athena.DataModel.Core;
using CommunityToolkit.Maui.Storage;

namespace Athena.UI
{
    internal interface IBackupService
    {
        /// <summary>
        /// Asynchronously creates a backup.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<FileSaverResult?> Create(IContext context);

        /// <summary>
        /// Asynchronously restores a backup.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <param name="requireUserConfirmation"></param>
        /// <param name="showProgressIndicator"></param>
        /// <returns></returns>
        Task<BackupRestoreResult> Restore(
            IContext context,
            string path,
            Func<string, string, string, Task<bool>> requireUserConfirmation, Action<bool> showProgressIndicator);
    }
}
