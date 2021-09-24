using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SymbolGuessing.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                return number.ToString();
            }

            return DependencyProperty.UnsetValue;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (double.TryParse(str, out double number))
                {
                    return number;
                }
                else
                {
                    return default(double);
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
