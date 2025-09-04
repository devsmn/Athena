using Athena.DataModel.Core;

namespace Athena.Data.Core
{
    public interface IDataIntegrityValidator
    {
        /// <summary>
        /// Validates the integrity of the given data source.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cipher"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        Task<bool> ValidateAsync(IContext context, string cipher, string db);
    }
}
