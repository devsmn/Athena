using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    internal partial class LanguageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _id;

        public LanguageViewModel(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }
}
