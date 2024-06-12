using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Page = Athena.DataModel.Page;

namespace Athena.UI
{
    public partial class MoveDocumentTargetViewModel : ObservableObject
    {
        public bool IsFolder { get; private set; }
        public Page Page { get; private set; }

        [ObservableProperty]
        private ObservableCollection<MoveDocumentTargetViewModel> _children;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _comment;

        public MoveDocumentTargetViewModel(string name, string comment, bool isFolder, Page page)
        {
            Name = name;
            IsFolder = isFolder;
            Comment = comment;
            Children = new();
            Page = page;
        }
    }
}
