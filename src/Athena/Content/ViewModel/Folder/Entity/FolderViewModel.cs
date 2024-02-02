using System.Collections.ObjectModel;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Athena.DataModel;
    using System.ComponentModel.DataAnnotations;

    public partial class FolderViewModel : ObservableObject
    {
        private readonly Folder _folder;
        private ObservableCollection<PageViewModel> _pages;
        

        [Display(AutoGenerateField = false)]
        public DateTime CreationDate
        {
            get { return _folder.CreationDate; }
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

        [Display(AutoGenerateField = false)]
        public ObservableCollection<PageViewModel> Pages
        {
            get
            {
                if (_pages == null)
                {
                    _pages = new ObservableCollection<PageViewModel>(_folder.Pages.Select(x => new PageViewModel(x)));
                }

                return _pages;
            }

            set
            {
                _pages = value;
                OnPropertyChanged();
            }
        }

        public FolderViewModel(Folder folder)
        {
            _folder = folder;
        }

        public static implicit operator Folder(FolderViewModel viewModel)
        {
            return viewModel.Folder;
        }

        public static implicit operator FolderViewModel(Folder folder)
        {
            return new FolderViewModel(folder);
        }

        public void AddPage(PageViewModel page)
        {
            Pages.Add(page);
        }

        public void RemovePage(PageViewModel page)
        {
            Pages.Remove(page);

            _folder.Pages.Remove(page.Page);
        }
    }
}
