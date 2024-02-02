using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace Athena.UI
{
    public partial class ContextViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private bool _showActivityIndicator;

        public ContextViewModel()
            : base()
        {
            // TODO: bool if sub is needed?
            ServiceProvider.GetService<IDataBrokerService>().Published += OnDataBrokerPublished;
            ServiceProvider.GetService<IDataBrokerService>().PublishStarted += OnDataBrokerPublishStarted;
            ServiceProvider.GetService<IDataBrokerService>().AppInitialized += OnDataBrokerAppInitialized;
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

        protected async Task PushAsync(Microsoft.Maui.Controls.Page page)
        {
            await ServiceProvider.GetService<INavigationService>().PushAsync(page);
        }

        protected async Task PopAsync()
        {
            await ServiceProvider.GetService<INavigationService>().PopAsync();
        }

        protected async Task PushModalAsync(Microsoft.Maui.Controls.Page page)
        {
            await ServiceProvider.GetService<INavigationService>().PushModalAsync(page);
        }

        protected async Task PopModalAsync()
        {
            await ServiceProvider.GetService<INavigationService>().PopModalAsync();
        }

        protected async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await ServiceProvider.GetService<INavigationService>().DisplayAlert(
                title,
                message,
                accept,
                cancel);
        }

        protected async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            return await ServiceProvider.GetService<INavigationService>().DisplayPrompt(
                title,
                message,
                ok,
                cancel);
        }

        protected async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return await ServiceProvider.GetService<INavigationService>().DisplayActionSheet(
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
                ServiceProvider.GetService<IDataBrokerService>().Published -= OnDataBrokerPublished;
                ServiceProvider.GetService<IDataBrokerService>().PublishStarted -= OnDataBrokerPublishStarted;
            }
        }
    }
}
