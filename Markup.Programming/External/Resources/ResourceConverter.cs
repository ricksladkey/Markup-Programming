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
    [ContentProperty("Expressions")]
    public class ResourceConverter : ResourceComponent, IValueConverter
    {
        public ResourceConverter()
        {
            Expressions = new ExpressionCollection();
        }

        public string ConvertPath { get; set; }

        private PathExpression convertPathExpression = new PathExpression();
        protected PathExpression ConvertPathExpression { get { return convertPathExpression; } }

        public string ConvertBackPath { get; set; }

        private PathExpression convertBackPathExpression = new PathExpression();
        protected PathExpression ConvertBackPathExpression { get { return convertBackPathExpression; } }

        public ExpressionCollection Expressions
        {
            get { return (ExpressionCollection)GetValue(ExpressionsProperty); }
            set { SetValue(ExpressionsProperty, value); }
        }

        public static readonly DependencyProperty ExpressionsProperty =
            DependencyProperty.Register("Expressions", typeof(ExpressionCollection), typeof(ResourceConverter), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(Expressions);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            return Evaluate(Expressions.Count >= 1 ? Expressions[0] : null,
                ConvertPath, ConvertPathExpression, value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TryToAttach();
            return Evaluate(Expressions.Count >= 2 ? Expressions[1] : null,
                ConvertBackPath, ConvertBackPathExpression, value, targetType, parameter, culture);
        }

        public object Evaluate(IExpression expression,
            string path, PathExpression pathExpression, object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = new NameDictionary
            {
                { "@Value", value },
                { "@TargetType", targetType },
                { "@Parameter", parameter },
                { "@Culture", culture },
            };
            if (path != null) return new Engine().With(this, parameters, engine => engine.GetPath(path, pathExpression));
            return new Engine().With(this, parameters, engine => expression.Evaluate(engine));
        }
    }
}
