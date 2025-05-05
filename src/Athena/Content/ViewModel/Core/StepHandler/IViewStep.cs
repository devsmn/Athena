using Athena.DataModel.Core;

namespace Athena.UI
{
    public interface IViewStep<in TViewModel>
         where TViewModel : ContextViewModel
    {
        Task ExecuteAsync(IContext context, TViewModel vm);
    }
}
