using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;

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
        }

        [RelayCommand]
        private async Task NextStep()
        {
            IContext context = this.RetrieveContext();

            _parentFolder.AddFolder(Folder);
            _parentFolder.Save(context);

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, IsNew ? UpdateType.Add : UpdateType.Edit, _parentFolder?.Key);
            await PopAsync();
            NewFolderStep = 0;
        }
    }
}
