namespace Athena.UI
{
    internal class DefaultNavigationService : INavigationService
    {
        private readonly Stack<Page> _pages = new();

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            Page currentPage = _pages.Peek();
            return await currentPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task<string> DisplayPrompt(string title, string message, string ok, string cancel)
        {
            Page currentPage = _pages.Peek();
            return await currentPage.DisplayPromptAsync(title, message, ok, cancel);
        }

        public async Task<string> DisplayPrompt(string title, string message, string ok, string cancel, Keyboard keyboard)
        {
            Page currentPage = _pages.Peek();
            return await currentPage.DisplayPromptAsync(title, message, ok, cancel, keyboard: keyboard);
        }

        public async Task PushAsync(Page page)
        {
            Page currentPage = _pages.Peek();
            await currentPage.Navigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            Page currentPage = _pages.Peek();
            await currentPage.Navigation.PopAsync();
        }

        public async Task PushModalAsync(Page page)
        {
            Page currentPage = _pages.Peek();
            await currentPage.Navigation.PushModalAsync(page);
        }

        public async Task PopModalAsync()
        {
            Page currentPage = _pages.Peek();
            await currentPage.Navigation.PopModalAsync();
        }

        public void AsRoot(Page page)
        {
            _pages.Push(page);
        }

        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            Page currentPage = _pages.Peek();

            return await currentPage.DisplayActionSheet(
                title,
                cancel,
                destruction,
                buttons);
        }
    }
}
