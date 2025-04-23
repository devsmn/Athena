using System.ComponentModel;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using DataModel;

    internal partial class FolderEditorViewModel : ContextViewModel
    {
        private readonly Folder _parentFolder;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private FolderViewModel _folder;

        [ObservableProperty]
        private int _newFolderStep;

        public FolderEditorViewModel(Folder folderToEdit, Folder parentFolder)
        {
            if (folderToEdit == null)
            {
                folderToEdit = new Folder();
                IsNew = true;
            }

            _parentFolder = parentFolder;
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

            if (IsNew)
            {
                _parentFolder.AddFolder(Folder);
                _parentFolder.Save(context);
            }
            else
            {
                Folder.Folder.Save(context, FolderSaveOptions.Folder);
            }

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, IsNew ? UpdateType.Add : UpdateType.Edit, _parentFolder?.Key);
            await PopAsync();
            Folder.PropertyChanged -= FolderOnPropertyChanged;
            NewFolderStep = 0;
        }
    }
}
