using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace WaybillWpf.ViewModels.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        // Получаем поле enum
        FieldInfo field = value.GetType().GetField(value.ToString());
        
        // Получаем атрибут Description
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        // Возвращаем описание, если оно есть, иначе само значение строкой
        return attribute != null ? attribute.Description : value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}