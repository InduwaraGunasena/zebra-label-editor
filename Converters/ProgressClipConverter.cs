using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace zebra_label_editor.Converters
{
    // Converts progress data to a rectangular Geometry clip for the white percentage text
    public class ProgressClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return Binding.DoNothing;

            if (!(values[0] is double totalWidth)) return Binding.DoNothing;
            if (!(values[1] is double totalHeight)) return Binding.DoNothing;

            double progress = 0;
            if (values[2] is double d) progress = d;
            else if (values[2] is int i) progress = i;
            else if (values[2] is string s && double.TryParse(s, out var parsed)) progress = parsed;

            // Clamp
            progress = Math.Max(0, Math.Min(100, progress));

            double clipWidth = (progress / 100.0) * totalWidth;

            // Return a RectangleGeometry starting at 0,0 to clipWidth x totalHeight
            return new RectangleGeometry(new Rect(0, 0, clipWidth, totalHeight));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
