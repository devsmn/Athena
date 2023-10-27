using CommunityToolkit.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public class ByteArrayToSafeImageSourceConverter : ByteArrayToImageSourceConverter
    {
        private readonly ImageSource emptyFolderImage = ImageSource.FromFile("empty_folder.png");
        public ByteArrayToSafeImageSourceConverter() : base()
        {
        }

        [return: NotNullIfNotNull("value")]
        public override ImageSource ConvertFrom(byte[] value, CultureInfo culture = null)
        {
            if (value == null || value.Length == 0)
                return emptyFolderImage;

            return base.ConvertFrom(value, culture);
        }

    }
}
