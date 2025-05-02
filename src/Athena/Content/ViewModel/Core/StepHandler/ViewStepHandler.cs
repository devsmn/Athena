using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class ViewStepHandler<TViewModel> : ObservableObject
        where TViewModel : ContextViewModel
    {
        private readonly Dictionary<int, IViewStep<TViewModel>> _steps = new();
        private readonly TViewModel _vm;

        [ObservableProperty]
        private int _stepIndex;

        public ViewStepHandler(TViewModel vm)
        {
            _vm = vm;
        }

        public void Register<TStep>(int index, TStep step) where TStep : IViewStep<TViewModel>
        {
            _steps.Add(index, step);
        }

        public async Task Next(IContext context)
        {
            int tmpIndex = StepIndex;

            if (_steps.TryGetValue(++tmpIndex, out IViewStep<TViewModel> step))
            {
                StepIndex = tmpIndex;
                await step.ExecuteAsync(context, _vm);
            }
        }

        public async Task<bool> Back(IContext context)
        {
            int tmpIndex = StepIndex;

            if (!_steps.TryGetValue(--tmpIndex, out IViewStep<TViewModel> step))
                return false;

            StepIndex = tmpIndex;
            await step.ExecuteAsync(context, _vm);
            return true;
        }
    }
}
