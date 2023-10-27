using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Android.Content;
    using Athena.DataModel;

    public partial class FolderViewModel : ObservableObject
    {
        private readonly Folder _folder;
        private ObservableCollection<PageViewModel> _pages;

        [ObservableProperty]
        private int _pageCount;

        public Folder Folder
        {
            get { return _folder; }
        }

        public int Id
        {
            get { return _folder.Key.Id; }
        }

        public string Name
        {
            get { return _folder.Name; }
            set
            {
                _folder.Name = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return _folder.Comment; }
            set
            {
                _folder.Comment = value;
                OnPropertyChanged();
            }
        }

        public byte[] Thumbnail
        {
            get { return _folder.Thumbnail; }
            set
            {
                _folder.Thumbnail = value;
                OnPropertyChanged();
            }
        }
        
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
            PageCount = _folder.Pages.Count;
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
            PageCount++;
        }

        public void RemovePage(PageViewModel page)
        {
            Pages.Remove(page);
            PageCount--;

            _folder.Pages.Remove(page.Page);
        }
    }
}
