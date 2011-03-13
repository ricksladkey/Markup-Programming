using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ArgumentsExpression is an expression with an Arguments property
    /// as its default content.  This is the base class for Call and New.
    /// </summary>
    [ContentProperty("Arguments")]
    public abstract class ArgumentsExpression : ExpressionBase
    {
        public ArgumentsExpression()
        {
            Arguments = new ExpressionCollection();
        }

        public ExpressionCollection Arguments
        {
            get { return (ExpressionCollection)GetValue(ArgumentsProperty); }
            set { SetValue(ArgumentsProperty, value); }
        }

        public static readonly DependencyProperty ArgumentsProperty =
            DependencyProperty.Register("Arguments", typeof(ExpressionCollection), typeof(ArgumentsExpression), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ArgumentsProperty);
        }
    }
}
