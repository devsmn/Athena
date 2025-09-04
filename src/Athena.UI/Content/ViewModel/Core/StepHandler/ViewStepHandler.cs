using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    /// <summary>
    /// Provides common methods for handling step-based views. 
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public partial class ViewStepHandler<TViewModel> : ObservableObject
        where TViewModel : ContextViewModel
    {
        private readonly Dictionary<int, IViewStep<TViewModel>> _steps = new();
        private readonly HashSet<int> _increaseSteps = new();
        private readonly TViewModel _vm;

        [ObservableProperty]
        private int _stepIndex;

        public ViewStepHandler(TViewModel vm)
        {
            _vm = vm;
        }

        /// <summary>
        /// Registers a new <see cref="IViewStep{TViewModel}"/> that executes a certain action.
        /// </summary>
        /// <typeparam name="TStep"></typeparam>
        /// <param name="index"></param>
        /// <param name="step"></param>
        public void Register<TStep>(int index, TStep step) where TStep : IViewStep<TViewModel>
        {
            _steps.Add(index, step);
        }

        /// <summary>
        /// Registers a new increase/decrease only step. Mainly used for binding different view states.
        /// </summary>
        /// <param name="index"></param>
        public void RegisterIncrease(int index)
        {
            _increaseSteps.Add(index);
        }

        /// <summary>
        /// Asynchronously executes the next step.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Next(IContext context)
        {
            int tmpIndex = StepIndex;
            tmpIndex++;

            if (_increaseSteps.Contains(tmpIndex))
            {
                StepIndex = tmpIndex;
            }
            else if (_steps.TryGetValue(tmpIndex, out IViewStep<TViewModel> step))
            {
                StepIndex = tmpIndex;
                await step.ExecuteAsync(context, _vm);
            }
        }

        /// <summary>
        /// Asynchronously executes the previous step.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> Back(IContext context)
        {
            int tmpIndex = StepIndex;
            tmpIndex--;

            if (_increaseSteps.Contains(tmpIndex))
            {
                StepIndex = tmpIndex;
                return true;
            }

            if (!_steps.TryGetValue(tmpIndex, out IViewStep<TViewModel> step))
                return false;

            StepIndex = tmpIndex;
            await step.ExecuteAsync(context, _vm);
            return true;
        }
    }
}
