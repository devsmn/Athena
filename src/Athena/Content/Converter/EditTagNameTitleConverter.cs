using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using Athena.UI;

namespace Athena.UI
{
    public class EditTagNameTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TagViewModel vm)
                return string.Empty;

            return vm.Id == IntegerEntityKey.TemporaryId ? Localization.NewTag : vm.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
