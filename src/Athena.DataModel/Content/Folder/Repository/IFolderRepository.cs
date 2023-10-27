using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public interface IFolderRepository : IAthenaRepository
    {
        void AddPage(IContext context, Folder folder, Page page);

        IEnumerable<Folder> ReadAll(IContext context);

        IEnumerable<Page> ReadAllPages(IContext context, Folder folder);

        void Save(IContext context, Folder folder);
    }
}
