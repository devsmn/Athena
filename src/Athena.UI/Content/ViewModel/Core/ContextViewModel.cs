using System.Diagnostics;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Athena.UI
{
    public partial class ContextViewModel : ObservableObject
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

        protected virtual void OnAppInitialized()
        {
            // Nothing to do.
        }

        protected virtual void OnPublishDataStarted()
        {
            // Nothing to do.
        }

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

        protected async Task PushAsync(Page page)
        {
            await Services.GetService<INavigationService>().PushAsync(page);
        }

        protected async Task PopAsync()
        {
            await Services.GetService<INavigationService>().PopAsync();
        }

        protected async Task PushModalAsync(Page page)
        {
            await Services.GetService<INavigationService>().PushModalAsync(page);
        }

        protected async Task PopModalAsync()
        {
            await Services.GetService<INavigationService>().PopModalAsync();
            DoneTcs?.SetResult(); // TODO (SPF): Only works if no other pages are pushed.
        }

        protected async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await Services.GetService<INavigationService>().DisplayAlert(
                title,
                message,
                accept,
                cancel);
        }

        protected async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            return await Services.GetService<INavigationService>().DisplayPrompt(
                title,
                message,
                ok,
                cancel);
        }

        protected async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return await Services.GetService<INavigationService>().DisplayActionSheet(
                title,
                cancel,
                destruction,
                buttons);
        }

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
    }
}
