using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public partial class OcrLanguage : ObservableObject
    {
        public bool IsInstalled { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public double Size { get; set; }
        public string DisplayName { get; set; }

        public OcrLanguage(string code, string name, double size)
        {
            Name = name;
            ShortName = code;
            Size = size;

            DisplayName = $"{Name} ({ShortName})";
        }
    }
}
