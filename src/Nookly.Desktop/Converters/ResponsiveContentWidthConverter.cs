using System.Globalization;
using System.Windows.Data;

namespace Nookly.Desktop.Converters;

public sealed class ResponsiveContentWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double availableWidth || double.IsNaN(availableWidth))
        {
            return double.NaN;
        }

        return availableWidth <= 1000
            ? availableWidth
            : Math.Min(availableWidth * 0.72, 1400);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
