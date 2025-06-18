using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GetFitterGetBigger.Views.Converters
{
    public class ObjectToBrushConverter : IValueConverter
    {
        public static ObjectToBrushConverter Instance = new ObjectToBrushConverter();

        public Brush ActiveBrush { get; set; }

        public Brush InactiveBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.ToString() == parameter.ToString())
            {
                return this.ActiveBrush;
            }
            return this.InactiveBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
