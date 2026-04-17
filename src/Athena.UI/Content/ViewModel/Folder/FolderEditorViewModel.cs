using System.ComponentModel;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Athena.DataModel;

namespace Athena.UI
{
    internal partial class FolderEditorViewModel : ContextViewModel
    {
        [ObservableProperty]
        private RootItemViewModel _rootItem;

        [ObservableProperty]
        private int _newFolderStep;

        private readonly TaskCompletionSource _doneTcs;

        public FolderEditorViewModel(RootItemViewModel toEdit, TaskCompletionSource doneTcs)
        {
            _doneTcs = doneTcs;
            RootItem = toEdit;
            RootItem.PropertyChanged += FolderOnPropertyChanged;
            NextStepCommand.NotifyCanExecuteChanged();
        }

        private void FolderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(RootItem.Name))
            {
                NextStepCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanExecuteNextStep()
        {
            return !string.IsNullOrWhiteSpace(RootItem.Name);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteNextStep))]
        private async Task NextStep()
        {
            IContext context = RetrieveContext();
            RootItem.Folder.Folder.Save(context, FolderSaveOptions.Folder);

            _doneTcs.SetResult();
            await PopAsync();
            RootItem.PropertyChanged -= FolderOnPropertyChanged;
            NewFolderStep = 0;
        }
    }
}
