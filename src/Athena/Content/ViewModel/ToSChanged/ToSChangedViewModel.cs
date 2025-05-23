using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class ToSChangedViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _isConfirmChecked;

        public ToSChangedViewModel()
        {

        }

        partial void OnIsConfirmCheckedChanged(bool value)
        {
            ConfirmClickedCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteConfirm()
        {
            return IsConfirmChecked;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteConfirm))]
        private async Task ConfirmClicked()
        {
            await PopModalAsync();
        }
    }
}
