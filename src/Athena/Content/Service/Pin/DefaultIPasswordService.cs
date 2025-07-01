using Athena.DataModel.Core;

namespace Athena.UI
{
    internal class DefaultIPasswordService : IPasswordService
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

        public async Task Prompt(IContext context, Action<string> passwordEntered)
        {
            INavigationService navService = Services.GetService<INavigationService>();

            string pw = string.Empty;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                pw = await navService.DisplayPrompt(
                    "Enter password",
                    "Unlock access to your data by entering your password",
                    "Unlock",
                    "Close",
                    Keyboard.Password);
            });

            if (!string.IsNullOrEmpty(pw))
            {
                passwordEntered(pw);
            }
        }
    }
}
