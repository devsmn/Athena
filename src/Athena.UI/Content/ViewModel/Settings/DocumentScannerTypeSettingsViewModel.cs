using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class DocumentScannerTypeSettingsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _showSettingsHint;

        [ObservableProperty]
        private bool _useAdvancedScanner;

        private readonly IPreferencesService _prefService;

        public DocumentScannerTypeSettingsViewModel(bool fromSettings, TaskCompletionSource tcs)
        {
            ShowSettingsHint = !fromSettings;
            DoneTcs = tcs;

            _prefService = Services.GetService<IPreferencesService>();

            if (_prefService.IsFirstScannerUsage())
                UseAdvancedScanner = true;
            else
                UseAdvancedScanner = _prefService.GetUseAdvancedScanner();
        }

        [RelayCommand]
        private async Task Close(object para)
        {
            _prefService.SetUseAdvancedScanner(UseAdvancedScanner);
            _prefService.SetFirstScannerUsage();
            await PopModalAsync();
        }
    }
}
