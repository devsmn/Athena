using Athena.DataModel.Core;

namespace Athena.UI
{
    /// <summary>
    /// Represents a single step of a <see cref="ViewStepHandler{TViewModel}"/>.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public interface IViewStep<in TViewModel>
         where TViewModel : ContextViewModel
    {
        /// <summary>
        /// Asynchronously executes this step for the given <paramref name="vm"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        Task ExecuteAsync(IContext context, TViewModel vm);
    }
}
