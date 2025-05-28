using System.Diagnostics;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class ContextViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;

        public ContextViewModel()
            : base()
        {
            Services.GetService<IDataBrokerService>().Published += OnDataBrokerPublished;
            Services.GetService<IDataBrokerService>().PublishStarted += OnDataBrokerPublishStarted;
            Services.GetService<IDataBrokerService>().AppInitialized += OnDataBrokerAppInitialized;
        }

        private async void OnDataBrokerAppInitialized(object sender, EventArgs e)
        {
            await OnAppInitialized();
        }

        protected virtual async Task OnAppInitialized()
        {
            // Nothing to do.
        }

        private void OnDataBrokerPublishStarted(object sender, EventArgs e)
        {
            OnPublishDataStarted();
        }

        protected virtual void OnPublishDataStarted()
        {
            // Nothing to do.
        }

        private void OnDataBrokerPublished(object sender, DataPublishedEventArgs e)
        {
            OnDataPublished(e);
        }

        protected virtual void OnDataPublished(DataPublishedEventArgs e)
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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Services.GetService<IDataBrokerService>().Published -= OnDataBrokerPublished;
                Services.GetService<IDataBrokerService>().PublishStarted -= OnDataBrokerPublishStarted;
                Services.GetService<IDataBrokerService>().AppInitialized -= OnDataBrokerAppInitialized;
            }
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
