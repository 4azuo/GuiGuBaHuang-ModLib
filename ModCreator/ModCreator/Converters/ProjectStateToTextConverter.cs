using System;
using System.Globalization;
using System.Windows.Data;
using ModCreator.Enums;

namespace ModCreator.Converters
{
    /// <summary>
    /// Converts ProjectState to display text
    /// </summary>
    public class ProjectStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectState state)
            {
                switch (state)
                {
                    case ProjectState.ProjectNotFound:
                        return "❌ Not Found";
                    case ProjectState.Valid:
                        return "✅ Valid";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
