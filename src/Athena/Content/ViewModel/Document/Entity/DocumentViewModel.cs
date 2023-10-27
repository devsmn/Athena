using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.LifecycleEvents;

namespace Athena.UI
{
    using Athena.DataModel;

    public class DocumentViewModel : ObservableObject
    {
        private readonly Document _document;

        public Document Document
        {
            get { return _document; }
        }

        public string Name
        {
            get { return _document.Name; }
            set
            {
                _document.Name = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return _document.Comment; }
            set
            {
                _document.Comment = value;
                OnPropertyChanged();
            }
        }

        public byte[] Image
        {
            get { return _document.Image; }
            set
            {
                _document.Image = value;
                OnPropertyChanged();
            }
        }


        public DocumentViewModel(Document document)
        {
            _document = document;
        }

        public static implicit operator DocumentViewModel(Document document)
        {
            return new DocumentViewModel(document);
        }

        public static implicit operator Document(DocumentViewModel document)
        {
            return document._document;
        }
    }
}
