using System.Globalization;

namespace Athena.UI
{
    public class IssueLevelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ReportIssueLevel level)
                return string.Empty;

            switch (level)
            {
                case ReportIssueLevel.Error:
                    return "error_color.png";

                case ReportIssueLevel.Info:
                    return "info.png";

                case ReportIssueLevel.Warning:
                    return "warning_color.png";

                case ReportIssueLevel.Success:
                    return "success_color.png";

                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
