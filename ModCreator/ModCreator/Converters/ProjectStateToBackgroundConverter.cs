using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ModCreator.Enums;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts ProjectState to background color
    /// </summary>
    public class ProjectStateToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectState state)
            {
                switch (state)
                {
                    case ProjectState.ProjectNotFound:
                        return new SolidColorBrush(Color.FromRgb(255, 200, 200)); // Light red
                    case ProjectState.Valid:
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
