using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming.Core
{
    [ContentProperty("Value")]
    public abstract class ValueExpression : TypedExpession
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(ValueExpression), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty);
        }
    }
}
