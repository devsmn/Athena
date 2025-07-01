using Athena.DataModel.Core;

namespace Athena.UI
{
    internal class DefaultIPasswordService : IPasswordService
    {
        public async Task New(IContext context, Action<string> onNewPasswordEntered)
        {
            TaskCompletionSource<string> tcs = new();

            INavigationService navService = Services.GetService<INavigationService>();
            await navService.PushModalAsync(new PinView(tcs, true));

            string pin = await tcs.Task;

            if (!string.IsNullOrEmpty(pin))
            {
                onNewPasswordEntered(pin);
            }
        }

        public async Task Prompt(IContext context, Action<string> passwordEntered)
        {
            TaskCompletionSource<string> tcs = new();

            INavigationService navService = Services.GetService<INavigationService>();
            await navService.PushModalAsync(new PinView(tcs));

            string pin = await tcs.Task;

            if (!string.IsNullOrEmpty(pin))
            {
                passwordEntered(pin);
            }
        }
    }
}
