using System;
using System.Globalization;
using System.Windows.Data;
using Markup.Programming.Core;

namespace Markup.Programming.Core
{
    public class ConverterInterop : Interop, IValueConverter
    {
        public ConverterInterop(IInteropHost parent) { Parent = parent; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Callback("Convert", value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Callback("ConvertBack", value, targetType, parameter, culture);
        }
    }
}
