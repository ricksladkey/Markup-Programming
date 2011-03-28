using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public class OperationsConverter: TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
                return new HandlerCollection { new EventHandler { Path = value as string } };
            return base.ConvertFrom(context, culture, value);
        }
    }
}
