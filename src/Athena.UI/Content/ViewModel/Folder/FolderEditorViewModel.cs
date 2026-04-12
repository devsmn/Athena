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
        private bool _isNew;

        [ObservableProperty]
        private FolderViewModel _folder;

        [ObservableProperty]
        private int _newFolderStep;

        private readonly TaskCompletionSource _doneTcs;

        public FolderEditorViewModel(FolderViewModel folderToEdit, TaskCompletionSource doneTcs)
        {
            _doneTcs = doneTcs;
            if (folderToEdit == null)
            {
                folderToEdit = new FolderViewModel(new Folder());
                IsNew = true;
            }

            Folder = folderToEdit;
            Folder.PropertyChanged += FolderOnPropertyChanged;
            NextStepCommand.NotifyCanExecuteChanged();
        }

        private void FolderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Folder.Name))
            {
                NextStepCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanExecuteNextStep()
        {
            return !string.IsNullOrWhiteSpace(Folder.Name);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteNextStep))]
        private async Task NextStep()
        {
            IContext context = RetrieveContext();

            if (!IsNew)
            {
                Folder.Folder.Save(context, FolderSaveOptions.Folder);
            }

            //Services.GetService<IDataBrokerService>().Publish(context, Folder.Folder, IsNew ? UpdateType.Add : UpdateType.Edit, _parentFolder?.Key);
            await PopAsync();
            Folder.PropertyChanged -= FolderOnPropertyChanged;
            NewFolderStep = 0;
            _doneTcs.SetResult();
        }
    }
}
