using System;
using System.Globalization;
using System.Windows.Data;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts boolean to folder/file icon
    /// </summary>
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFolder)
            {
                return isFolder ? "📁" : "📄";
            }
            return "📄";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
