using CommunityToolkit.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public class ByteArrayToSafeDocumentImageSourceConverter : ByteArrayToImageSourceConverter
    {
        // TODO: Override default values
        private readonly ImageSource _documentImage = ImageSource.FromFile("document.png");
        public ByteArrayToSafeDocumentImageSourceConverter() : base()
        {
        }

        [return: NotNullIfNotNull("value")]
        public override ImageSource ConvertFrom(byte[] value, CultureInfo culture = null)
        {
            if (value == null || value.Length == 0)
                return _documentImage;

            return base.ConvertFrom(value, culture);
        }
    }
}
