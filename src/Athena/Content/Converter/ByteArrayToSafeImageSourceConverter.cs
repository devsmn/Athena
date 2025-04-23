using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Athena.UI
{
    public class ByteArrayToSafeImageSourceConverter : ByteArrayToImageSourceConverter
    {
        // TODO: Override default values
        private readonly ImageSource _emptyFolderImage = ImageSource.FromFile("empty_folder.png");
        public ByteArrayToSafeImageSourceConverter() : base()
        {
        }

        [return: NotNullIfNotNull("value")]
        public override ImageSource ConvertFrom(byte[] value, CultureInfo culture = null)
        {
            if (value == null || value.Length == 0)
                return _emptyFolderImage;

            return base.ConvertFrom(value, culture);
        }
    }
}
