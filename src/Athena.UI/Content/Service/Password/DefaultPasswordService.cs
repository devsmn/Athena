using Athena.DataModel.Core;
using Athena.Resources.Localization;

namespace Athena.UI
{
    internal class DefaultPasswordService : IPasswordService
    {
        public async Task New(IContext context, Action<string> onNewPasswordEntered)
        {
            TaskCompletionSource<string> tcs = new();

            INavigationService navService = Services.GetService<INavigationService>();
            await navService.PushModalAsync(new PinView(tcs));

            string pw = await tcs.Task;

            if (!string.IsNullOrEmpty(pw))
            {
                onNewPasswordEntered(pw);
            }
        }

        public async Task Prompt(IContext context, string text, bool isRetry, Action<string> passwordEntered, Action cancelled = null)
        {
            INavigationService navService = Services.GetService<INavigationService>();

            string pw = string.Empty;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                string preText = isRetry ? $"{Localization.PasswordIncorrect}{Environment.NewLine}{Environment.NewLine}" : string.Empty;

                pw = await navService.DisplayPrompt(
                    Localization.PasswordPrompt,
                    $"{preText}{text}",
                    "Ok",
                    Localization.Close,
                    Keyboard.Password);
            });

            if (string.IsNullOrEmpty(pw))
                cancelled?.Invoke();
            else 
                passwordEntered(pw);
        }
    }
}
