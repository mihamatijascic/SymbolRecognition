using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SymbolGuessing.Converters
{
    [ValueConversion(typeof(int),typeof(string))]
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return number.ToString();
            }

            return DependencyProperty.UnsetValue;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (int.TryParse(str, out int number))
                {
                    return number;
                }
                else
                {
                    return default(int);
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
