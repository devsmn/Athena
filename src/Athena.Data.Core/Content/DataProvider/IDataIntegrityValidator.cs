using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;

namespace Athena.Data.Core
{
    public interface IDataIntegrityValidator
    {
        Task<bool> ValidateAsync(IContext context, string cipher, string db);
    }
}
