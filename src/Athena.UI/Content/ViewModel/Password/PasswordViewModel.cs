using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class PasswordViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _passwordValid;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _password;

        private readonly TaskCompletionSource<string> _tcs;

        public PasswordViewModel(TaskCompletionSource<string> tcs)
        {
            _tcs = tcs;
            NextCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void PasswordChanged()
        {
            PasswordValid = !string.IsNullOrEmpty(Password) && string.Equals(Password, ConfirmPassword);
            NextCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteNextCommand()
        {
            return PasswordValid;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteNextCommand))]
        private async Task Next()
        {
            await PopModalAsync();
            _tcs.SetResult(Password);
        }
    }
}
