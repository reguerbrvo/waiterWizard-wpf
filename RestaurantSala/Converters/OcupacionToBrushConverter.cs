using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantSala.Converters
{
    public class OcupacionToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 ||
                values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
            {
                return Brushes.Gray;
            }

            if (!int.TryParse(values[0]?.ToString(), out var comensales) ||
                !int.TryParse(values[1]?.ToString(), out var capacidad) ||
                capacidad <= 0)
            {
                return Brushes.Gray;
            }

            var ratio = (double)comensales / capacidad;

            if (comensales >= capacidad)
                return new SolidColorBrush(Color.FromRgb(235, 87, 87));

            if (ratio >= 0.75)
                return new SolidColorBrush(Color.FromRgb(242, 153, 74));

            return new SolidColorBrush(Color.FromRgb(46, 204, 113));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
