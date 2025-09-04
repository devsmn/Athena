namespace Athena.UI
{
    internal interface INavigationService
    {
        /// <summary>
        /// Pushes the given <paramref name="page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task PushAsync(Page page);

        /// <summary>
        /// Pops the last <see cref="Page"/>.
        /// </summary>
        /// <returns></returns>
        Task PopAsync();

        /// <summary>
        /// Pushes the given <paramref name="page"/> as modal.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task PushModalAsync(Page page);

        /// <summary>
        /// Pops the previously pushed modal.
        /// </summary>
        /// <returns></returns>
        Task PopModalAsync();

        /// <summary>
        /// Displays an alert.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);

        /// <summary>
        /// Displays a prompt.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<string> DisplayPrompt(string title, string message, string ok, string cancel);

        /// <summary>
        /// Displays a prompt.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <param name="keyboard"></param>
        /// <returns></returns>
        Task<string> DisplayPrompt(string title, string message, string ok, string cancel, Keyboard keyboard);

        /// <summary>
        /// Displays an action sheet.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);

        /// <summary>
        /// Sets the given <paramref name="page"/> as the root.
        /// </summary>
        /// <param name="page"></param>
        void AsRoot(Page page);
    }
}
