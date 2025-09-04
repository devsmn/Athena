using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public class Backup
    {
        public BackupMetaData MetaData { get; private set; }

        public Backup(BackupMetaData metaData)
        {
            MetaData = metaData;
        }
    }
}
