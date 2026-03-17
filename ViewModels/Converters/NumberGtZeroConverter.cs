using System;
using System.Globalization;
using System.Windows.Data;

namespace WaybillWpf.ViewModels.Converters
{
    public class NumberGtZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dblValue)
            {
                return dblValue > 0;
            }
            if (value is float fltValue)
            {
                return fltValue > 0;
            }
            if (value is double dbValue)
            {
                return dbValue > 0;
            }
            if (value is int intValue)
            {
                return intValue > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
