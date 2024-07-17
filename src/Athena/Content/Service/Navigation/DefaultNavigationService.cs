namespace Athena.UI
{
    internal class DefaultNavigationService : INavigationService
    {
        private readonly Stack<Page> _pages = new();

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            var currentPage = _pages.Peek();
            return await currentPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            var currentPage = _pages.Peek();
            return await currentPage.DisplayPromptAsync(title, message, ok, cancel);
        }

        public async Task PushAsync(Page page)
        {
            var currentPage = _pages.Peek();
            await currentPage.Navigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            var currentPage = _pages.Peek();
            await currentPage.Navigation.PopAsync();
        }

        public async Task PushModalAsync(Page page)
        {
            var currentPage = _pages.Peek();
            await currentPage.Navigation.PushModalAsync(page);
        }

        public async Task PopModalAsync()
        {
            var currentPage = _pages.Peek();
            await currentPage.Navigation.PopModalAsync();
        }

        public void AsRoot(Page page)
        {
            _pages.Push(page);
        }

        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            var currentPage = _pages.Peek();
            
            return await currentPage.DisplayActionSheet(
                title,
                cancel,
                destruction,
                buttons);
        }
    }
}
