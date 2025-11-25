using System;
using System.Globalization;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converter to invert boolean values
    /// </summary>
    public class InverseBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
