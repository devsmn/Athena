namespace Athena.UI
{
    internal class DefaultNavigationService : INavigationService
    {
        private Page _root;
        private bool _setIsBusy;

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await _root.DisplayAlert(title, message, accept, cancel);
        }

        public async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            return await _root.DisplayPromptAsync(title, message, ok, cancel);
        }

        public async Task<string> DisplayPrompt(string title, string message, string ok, string cancel, Keyboard keyboard)
        {
            return await _root.DisplayPromptAsync(title, message, ok, cancel, keyboard: keyboard);
        }

        public async Task PushAsync(Page page)
        {
            await _root.Navigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            await _root.Navigation.PopAsync();
        }

        public async Task PushModalAsync(Page page)
        {
            ContextViewModel vm = _root.BindingContext as ContextViewModel;
            _setIsBusy = vm?.IsBusy ?? false;
            await _root.Navigation.PushModalAsync(page);
        }

        public async Task PopModalAsync()
        {
            await _root.Navigation.PopModalAsync();

            if (_setIsBusy && _root.BindingContext is ContextViewModel vm)
            {
                vm.IsBusy = false;
                await Task.Delay(10);
                vm.IsBusy = true;
            }

            _setIsBusy = false;
        }

        public void AsRoot(Page page)
        {
            _root = page;
        }

        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return await _root.DisplayActionSheet(
                title,
                cancel,
                destruction,
                buttons);
        }
    }
}
