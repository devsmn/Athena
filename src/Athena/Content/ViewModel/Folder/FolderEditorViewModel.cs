using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;

    internal partial class FolderEditorViewModel : ContextViewModel
    {
        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private FolderViewModel _folder;

        [ObservableProperty]
        private int _newFolderStep;

        public FolderEditorViewModel(Folder folder)
        {
            if (folder == null)
            {
                folder = new Folder();
                IsNew = true;
            }

            Folder = folder;
        }

        [RelayCommand]
        private async Task NextStep()
        {
            IContext context = this.RetrieveContext();

            Folder.Folder.Save(context);
            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Folder.Folder, IsNew ? UpdateType.Add : UpdateType.Edit);
            await PopAsync();
            NewFolderStep = 0;
        }
    }
}
