using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class ColorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _hex;

        [ObservableProperty]
        private string _name;

        public ColorViewModel(string name, string hex)
        {
            Hex = hex;
            Name = name;
        }
    }
}
