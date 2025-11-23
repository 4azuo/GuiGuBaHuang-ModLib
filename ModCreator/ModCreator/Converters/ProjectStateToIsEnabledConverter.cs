using System;
using System.Globalization;
using System.Windows.Data;
using ModCreator.Enums;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts ProjectState to IsEnabled boolean (Valid = true, NotFound = false)
    /// </summary>
    public class ProjectStateToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectState state)
            {
                return state == ProjectState.Valid;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
