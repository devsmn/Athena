using System.Diagnostics;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Athena.UI
{
    public partial class ContextViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;

        public TaskCompletionSource DoneTcs { get; protected set; }

        public ContextViewModel()
        {
            WeakReferenceMessenger.Default.Register<DataPublishedArgs>(this, (_, data) => OnDataPublished(data));
            WeakReferenceMessenger.Default.Register<DataPublishStartedMessage>(this, (_, _) => OnPublishDataStarted());
            WeakReferenceMessenger.Default.Register<AppInitializedMessage>(this, (_, _) => OnAppInitialized());
        }

        /// <summary>
        /// Invoked when the app is initialized.
        /// </summary>
        protected virtual void OnAppInitialized()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Invoked when the system starts to publish data.
        /// </summary>
        protected virtual void OnPublishDataStarted()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Invoked when data is published.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnDataPublished(DataPublishedArgs data)
        {
            //  Nothing to do.
        }

        [DebuggerStepThrough]
        protected IContext RetrieveContext()
        {
            return new AthenaAppContext();
        }

        [DebuggerStepThrough]
        protected IContext RetrieveReportContext()
        {
            return new ReportContext(Report);
        }

        private void Report(string message)
        {
            MainThread.BeginInvokeOnMainThread(() => BusyText = message);
        }

        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        protected async Task PushAsync(Page page)
        {
            await Services.GetService<INavigationService>().PushAsync(page);
        }

        /// <summary>
        /// Pops the active page from the navgiation stack.
        /// </summary>
        /// <returns></returns>
        protected async Task PopAsync()
        {
            await Services.GetService<INavigationService>().PopAsync();
        }

        /// <summary>
        /// Pushes the given <paramref name="page"/> as a modal page to the navigation stack.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        protected async Task PushModalAsync(Page page)
        {
            await Services.GetService<INavigationService>().PushModalAsync(page);
        }

        /// <summary>
        /// Pops the current modal page from the navgiation stack.
        /// </summary>
        /// <returns></returns>
        protected async Task PopModalAsync()
        {
            DoneTcs?.SetResult(); // TODO (SPF): Only works if no other pages are pushed.
            await Services.GetService<INavigationService>().PopModalAsync();
        }

        /// <summary>
        /// Displays an alert.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        protected async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await Services.GetService<INavigationService>().DisplayAlert(
                title,
                message,
                accept,
                cancel);
        }

        /// <summary>
        /// Displays a prompt.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        protected async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            return await Services.GetService<INavigationService>().DisplayPrompt(
                title,
                message,
                ok,
                cancel);
        }

        /// <summary>
        /// Displays a selection.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        protected async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return await Services.GetService<INavigationService>().DisplayActionSheet(
                title,
                cancel,
                destruction,
                buttons);
        }

        /// <summary>
        /// Executes an longer-running background action while displaying a wait indicator.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected async Task ExecuteAsyncBackgroundAction(Func<IContext, Task> action)
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = true);

            await Task.Run(async () =>
            {
                await Task.Delay(200);
                await action(RetrieveReportContext());
            });

            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }

        /// <summary>
        /// Executes an longer-running background action while displaying a wait indicator.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected async Task ExecuteBackgroundAction(Action<IContext> action)
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = true);

            await Task.Run(async () =>
            {
                await Task.Delay(100);
                action(RetrieveReportContext());
            });

            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {
        }
    }
}
