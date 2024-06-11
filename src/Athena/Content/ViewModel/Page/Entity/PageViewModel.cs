using System.Collections.ObjectModel;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Athena.DataModel;
    using System.ComponentModel.DataAnnotations;

    public partial class PageViewModel : ObservableObject
    {
        private readonly Page _page;
        private ObservableCollection<DocumentViewModel> _documents;

        [Display(AutoGenerateField = false)]
        public DateTime CreationDate
        {
            get { return _page.CreationDate; }
        }

        [Display(AutoGenerateField = false)]
        public DateTime ModDate
        {
            get { return _page.ModDate; }
        }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.PageVMTitleRequired))]
        [StringLength(45, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.PageVMTitleExceedCharLimit))]
        [Display(ResourceType = typeof(Localization), Name = nameof(Localization.PageVMTitle))]
        [DataType(DataType.Text)]
        public string Title
        {
            get { return _page.Title; }
            set
            {
                _page.Title = value;
                OnPropertyChanged();
            }
        }

        [Display(ResourceType = typeof(Localization), Name = nameof(Localization.PageVMComment))]
        [StringLength(80, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = nameof(Localization.PageVMCommentExceedCharLimit))]
        [DataType(DataType.MultilineText)]
        public string Comment
        {
            get { return _page.Comment; }
            set
            {
                _page.Comment = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (_documents == null)
                {
                    _documents = new ObservableCollection<DocumentViewModel>(_page.Documents.Select(x => new DocumentViewModel(x)));
                }

                return _documents;
            }

            set
            {
                this._documents = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public Page Page
        {
            get { return _page; }
        }

        // ---- constructor ----

        /// <summary>
        /// Initializes a new instance of <see cref="PageViewModel"/>.
        /// </summary>
        /// <param name="page"></param>
        public PageViewModel(Page page)
        {
            _page = page;
        }

        public static implicit operator Page(PageViewModel viewModel)
        {
            return viewModel.Page;
        }

        public static implicit operator PageViewModel(Page page)
        {
            return new PageViewModel(page);
        }

        public void AddDocument(Document document)
        {
            this.Documents.Add(document);
        }

        internal void RemoveDocument(DocumentViewModel document)
        {
            this.Documents.Remove(document);
            this._page.Documents.Remove(document);
        }
    }
}
