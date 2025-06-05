using System.ComponentModel.DataAnnotations;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class FolderViewModel : ObservableObject, IVisualModel<Folder>
    {
        private readonly Folder _folder;

        [Display(AutoGenerateField = false)]
        public IEnumerable<Document> LoadedDocuments
        {
            get { return _folder.LoadedDocuments; }
        }

        [Display(AutoGenerateField = false)]
        public IEnumerable<Folder> LoadedFolders
        {
            get { return _folder.LoadedFolders; }
        }

        [Display(AutoGenerateField = false)]
        public IEnumerable<Folder> Folders
        {
            get { return _folder.Folders; }
        }

        [Display(AutoGenerateField = false)]
        public IEnumerable<Document> Documents
        {
            get { return _folder.Documents; }
        }

        [Display(AutoGenerateField = false)]
        public DateTime CreationDate
        {
            get { return _folder.CreationDate; }
        }

        [Display(AutoGenerateField = false)]
        public IntegerEntityKey Key
        {
            get { return _folder.Key; }
        }

        [Display(AutoGenerateField = false)]
        public DateTime ModDate
        {
            get { return _folder.ModDate; }
        }

        [Display(AutoGenerateField = false)]
        public Folder Folder
        {
            get { return _folder; }
        }

        [Display(AutoGenerateField = false)]
        public int Id
        {
            get { return _folder.Key.Id; }
        }

        public void Edit(Folder entity)
        {
            Name = entity.Name;
            Comment = entity.Comment;
            IsPinned = entity.IsPinned;
        }

        [Display(AutoGenerateField = false)]
        public bool IsPinned
        {
            get { return _folder.IsPinned; }
            set
            {
                _folder.IsPinned = value;
                OnPropertyChanged();
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.FolderVMTitleRequired))]
        [StringLength(45, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.FolderVMTitleExceedCharLimit))]
        [Display(ResourceType = typeof(Localization), Name = nameof(Localization.FolderVMName))]
        [DataType(DataType.Text)]
        public string Name
        {
            get { return _folder.Name; }
            set
            {
                _folder.Name = value;
                OnPropertyChanged();
            }
        }

        [Display(ResourceType = typeof(Localization), Name = nameof(Localization.FolderVMComment))]
        [StringLength(80, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.FolderVMCommentExceedCharLimit))]
        [DataType(DataType.MultilineText)]
        public string Comment
        {
            get { return _folder.Comment; }
            set
            {
                _folder.Comment = value;
                OnPropertyChanged();
            }
        }

        public FolderViewModel(Folder folder)
        {
            _folder = folder;
        }

        public void DeleteFolder(Folder toDelete)
        {
            var delete = Folder.Folders.FirstOrDefault(x => x.Key.Id == toDelete.Id);

            if (delete == null)
                return;

            Folder.Folders.Remove(delete);
        }

        public void DeleteDocument(Document toDelete)
        {
            var delete = Folder.Documents.FirstOrDefault(x => x.Key.Id == toDelete.Id);

            if (delete == null)
                return;

            Folder.Documents.Remove(delete);
        }

        public static implicit operator Folder(FolderViewModel viewModel)
        {
            return viewModel.Folder;
        }

        public static implicit operator FolderViewModel(Folder folder)
        {
            return new FolderViewModel(folder);
        }
    }
}
