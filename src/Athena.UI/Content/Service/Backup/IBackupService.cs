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
        Task<FileSaverResult?> Create(IContext context);

        Task<BackupRestoreResult> Restore(
            IContext context,
            string path,
            Func<string, string, string, Task<bool>> requireUserConfirmation, Action<bool> showProgressIndicator);
    }
}
