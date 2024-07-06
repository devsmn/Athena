using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
using Athena.DataModel.Core;

namespace Athena.UI
{
    public class RootItem : Entity
    {
        public FolderViewModel Folder { get; set; }
        public DocumentViewModel Document { get; set; }

        public bool IsFolder { get; set; }

        public bool IsPinned
        {
            get { return IsFolder ? Folder.IsPinned : Document.IsPinned; }
            set
            {
                if (IsFolder)
                {
                    Folder.IsPinned = value;
                }
                else
                {
                    Document.IsPinned = value;
                }
            }
        }


        public string Name
        {
            get { return IsFolder ? Folder.Name : Document.Name; }
            set
            {
                if (IsFolder)
                {
                    Folder.Name = value;
                }
                else
                {
                    Document.Name = value;
                }
            }
        }

        public string Comment
        {
            get { return IsFolder ? Folder.Comment : Document.Comment; }
            set
            {
                if (IsFolder)
                {
                    Folder.Comment = value;
                }
                else
                {
                    Document.Comment = value;
                }
            }
        }

        //public int Id
        //{
        //    get { return IsFolder ? Folder.Id : Document.Id; }
        //}


        public RootItem(FolderViewModel folder)
             : base(folder.Key)
        {
            Folder = folder;
            IsFolder = true;
        }

        public RootItem(Folder folder)
            : this(new FolderViewModel(folder))
        {
        }

        public RootItem(DocumentViewModel document)
            : base(document.Key)
        {
            Document = document;
            IsFolder = false;
        }

        public RootItem(Document document)
            : this(new DocumentViewModel(document))
        {
        }
    }

    public class RootItemViewModel : ObservableObject, IVisualModel<RootItem>
    {
        private readonly RootItem _rootItem;

        [Display(AutoGenerateField = false)]
        public FolderViewModel Folder
        {
            get { return _rootItem.Folder; }
        }

        [Display(AutoGenerateField = false)]
        public DocumentViewModel Document
        {
            get { return _rootItem.Document; }
        }

        [Display(AutoGenerateField = false)]
        public bool IsFolder
        {
            get { return _rootItem.IsFolder; }
        }

        [Display(AutoGenerateField = false)]
        public bool IsPinned
        {
            get { return _rootItem.IsPinned; }
            set
            {
                _rootItem.IsPinned = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _rootItem.Name; }
            set
            {
                _rootItem.Name = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return _rootItem.Comment; }
            set
            {
                _rootItem.Comment = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public int Id
        {
            get { return _rootItem.Id; }
        }


        public RootItemViewModel(FolderViewModel folder)
        {
            _rootItem = new RootItem(folder);
        }

        public RootItemViewModel(Folder folder)
             : this(new FolderViewModel(folder))
        {
        }

        public RootItemViewModel(DocumentViewModel document)
        {
            _rootItem = new RootItem(document);
        }

        public RootItemViewModel(Document document)
            : this(new DocumentViewModel(document))
        {
        }

        public void Edit(RootItem entity)
        {
            if (IsFolder)
            {
                _rootItem.Folder.Edit(entity.Folder);
            }
            else
            {
                _rootItem.Document.Edit(entity.Document);
            }
        }
    }

}
