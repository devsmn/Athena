namespace Athena.UI
{
    internal interface INavigationService
    {
        Task PushAsync(Page page);
        Task PopAsync();
        Task PushModalAsync(Page page);
        Task PopModalAsync();
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
        Task<string> DisplayPrompt(string title, string message, string ok, string cancel);
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);


        void AsRoot(Page page);
    }
}
