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
            {
                UseAdvancedScanner = true;
                UseAdvancedScannerChanged();
            }
            else
            {
                UseAdvancedScanner = _prefService.GetUseAdvancedScanner();
            }
        }

        [RelayCommand]
        private void UseAdvancedScannerChanged()
        {
            _prefService.SetUseAdvancedScanner(UseAdvancedScanner);
            _prefService.SetFirstScannerUsage();
        }

        public async Task BackPressed()
        {
            await PopModalAsync();
            DoneTcs.SetResult();
        }
    }
}
