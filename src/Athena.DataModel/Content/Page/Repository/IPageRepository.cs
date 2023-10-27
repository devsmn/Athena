using Athena.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public interface IPageRepository : IAthenaRepository
    {
        void AddTag(Tag tag);

        void Save(IContext context, Page page);

        Page Read(IContext context, Page page);

        void Delete(IContext context, Page page);
        
        IEnumerable<Page> ReadAll(IContext context);

    }
}
