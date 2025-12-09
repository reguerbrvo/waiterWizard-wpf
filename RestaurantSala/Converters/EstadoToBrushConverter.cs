using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Converters
{
    public class EstadoToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EstadoMesa estado)
            {
                switch (estado)
                {
                    case EstadoMesa.Libre:
                        return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    case EstadoMesa.Reservada:
                        return new SolidColorBrush(Color.FromRgb(242, 201, 76));
                    case EstadoMesa.OcupadaSinComanda:
                        return new SolidColorBrush(Color.FromRgb(242, 153, 74));
                    case EstadoMesa.OcupadaConComanda:
                        return new SolidColorBrush(Color.FromRgb(235, 87, 87));
                }
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
