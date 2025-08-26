using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;
using Athena.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    internal partial class SecuritySettingsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _biometricsAvailable;

        public SecuritySettingsViewModel()
        {
            IHardwareKeyStoreService hardwareKeyStoreService = Services.GetService<IHardwareKeyStoreService>();
            BiometricsAvailable = hardwareKeyStoreService.BiometricsAvailable();
        }

        [RelayCommand]
        private async Task ChangePassword()
        {

        }
    }
}
