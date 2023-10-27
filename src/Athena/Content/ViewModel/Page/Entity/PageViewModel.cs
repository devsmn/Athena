using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Net;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class PageViewModel : ObservableObject
    {
        // ---- private fields ----
        private readonly Page _page;
        private ObservableCollection<DocumentViewModel> _documents;

        [ObservableProperty]
        private int _documentCount;

        // ---- public properties ----

        public string Title
        {
            get { return _page.Title; }
            set
            {
                _page.Title = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return _page.Comment; }
            set
            {
                _page.Comment = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (_documents == null)
                {
                    _documents = new ObservableCollection<DocumentViewModel>(_page.Documents.Select(x => new DocumentViewModel(x)));
                    DocumentCount = _documents.Count;
                }

                return _documents;
            }

            set
            {
                this._documents = value;
                OnPropertyChanged();
            }
        }
        
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
            DocumentCount = _page.Documents.Count;
        }

        // ---- methods ----

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
            DocumentCount++;
        }

        internal void RemoveDocument(DocumentViewModel document)
        {
            this.Documents.Remove(document);
            DocumentCount--;

            this._page.Documents.Remove(document);
        }
    }
}
