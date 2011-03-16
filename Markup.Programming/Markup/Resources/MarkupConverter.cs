using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A MarkupConverter is an IValueConverter that can be defined and
    /// referenced entirely in resources and by IExpression
    /// to implement the Convert and ConvertBack interface methods.
    /// </summary>
    public class MarkupConverter : ResourceObject, IValueConverter
    {
        public IExpression ConvertExpression
        {
            get { return (IExpression)GetValue(ConvertExpressionProperty); }
            set { SetValue(ConvertExpressionProperty, value); }
        }

        public static readonly DependencyProperty ConvertExpressionProperty =
            DependencyProperty.Register("ConvertExpression", typeof(IExpression), typeof(MarkupConverter), null);

        public string ConvertPath { get; set; }

        public IExpression ConvertBackExpression
        {
            get { return (IExpression)GetValue(ConvertBackExpressionProperty); }
            set { SetValue(ConvertBackExpressionProperty, value); }
        }

        public static readonly DependencyProperty ConvertBackExpressionProperty =
            DependencyProperty.Register("ConvertBackExpression", typeof(IExpression), typeof(MarkupConverter), null);

        public string ConvertBackPath { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ConvertExpressionProperty, ConvertBackExpressionProperty);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            return Evaluate(ConvertExpression, ConvertPath, value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            return Evaluate(ConvertBackExpression, ConvertBackPath, value, targetType, parameter, culture);
        }

        public object Evaluate(IExpression expression, string path, object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = new NameDictionary
            {
                { "ConverterValue", value },
                { "ConverterParameter", parameter },
                { "ConverterCulture", culture },
            };
            if (path != null) return new Engine().With(this, parameters, engine => engine.GetPath(path));
            return new Engine().With(this, parameters, engine => expression.Evaluate(engine));
        }
    }
}
