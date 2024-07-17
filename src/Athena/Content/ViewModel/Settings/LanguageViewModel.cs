using System.Globalization;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class LanguageViewModel : ObservableObject
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
