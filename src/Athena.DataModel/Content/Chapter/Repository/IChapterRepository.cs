using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public interface IChapterRepository : IAthenaRepository
    {
        IEnumerable<Chapter> Search(IContext context, string pattern);
        void Save(IContext context, Chapter chapter);
        void Delete(IContext context, int documentRef);
    }
}
