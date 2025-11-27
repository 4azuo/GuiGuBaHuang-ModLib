using ModCreator.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converter to convert file path to BitmapImage for Image control
    /// </summary>
    public class FilePathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var filePath = value.ToString();
            return BitmapHelper.LoadFromFile(filePath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
