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

        private PathExpression convertPathExpression = new PathExpression();
        protected PathExpression ConvertPathExpression { get { return convertPathExpression; } }

        public string ConvertBackPath { get; set; }

        private PathExpression convertBackPathExpression = new PathExpression();
        protected PathExpression ConvertBackPathExpression { get { return convertBackPathExpression; } }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            if (ConvertPath != null)
                return Evaluate(ConvertPath, ConvertPathExpression, value, targetType, parameter, culture);
            return interop.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            if (ConvertBackPath != null)
                return Evaluate(ConvertBackPath, ConvertPathExpression, value, targetType, parameter, culture);
            return interop.ConvertBack(value, targetType, parameter, culture);
        }

        public object Evaluate(string path, PathExpression pathExpression,
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = new NameDictionary
            {
                { "@Value", value },
                { "@TargetType", targetType },
                { "@Parameter", parameter },
                { "@Culture", culture },
            };
            return new Engine().With(this, parameters, engine => engine.GetPath(path, pathExpression));
        }
    }
}
