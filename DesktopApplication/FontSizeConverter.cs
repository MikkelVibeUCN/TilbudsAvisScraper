using System;
using System.Globalization;
using System.Windows.Data;

namespace DesktopApplication
{
    public class FontSizeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2
                && values[0] is double windowHeight
                && values[1] is double windowWidth)
            {
                // Calculate the scaling factors based on height and width
                double heightFactor = windowHeight * 0.03; // 5% of height
                double widthFactor = windowWidth * 0.03;   // 3% of width
                return (heightFactor + widthFactor) / 2;   // Average
            }
            return 12; // Default font size
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}