using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A ResourceConverter is an IValueConverter that can be defined and
    /// referenced entirely in resources and by IExpression
    /// to implement the Convert and ConvertBack interface methods.
    /// </summary>
    public class ResourceConverter : InterfaceComponent, IValueConverter
    {
        private IValueConverter interop;

        public ResourceConverter()
        {
            interop = new ConverterInterop(this);
        }

        public string ConvertPath { get; set; }

        private CodeTree convertCodeTree = new CodeTree();
        protected CodeTree ConvertCodeTree { get { return convertCodeTree; } }

        public string ConvertBackPath { get; set; }

        private CodeTree convertBackCodeTree = new CodeTree();
        protected CodeTree ConvertBackCodeTree { get { return convertBackCodeTree; } }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            if (ConvertPath != null)
                return Evaluate(ConvertPath, ConvertCodeTree, value, targetType, parameter, culture);
            return interop.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            if (ConvertBackPath != null)
                return Evaluate(ConvertBackPath, ConvertCodeTree, value, targetType, parameter, culture);
            return interop.ConvertBack(value, targetType, parameter, culture);
        }

        public object Evaluate(string path, CodeTree codeTree,
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = new NameDictionary
            {
                { "@Value", value },
                { "@TargetType", targetType },
                { "@Parameter", parameter },
                { "@Culture", culture },
            };
            return new Engine().FrameFunc(this, parameters, engine => engine.GetPath(path, codeTree));
        }
    }
}
