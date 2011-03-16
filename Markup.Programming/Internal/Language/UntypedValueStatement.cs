using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An UntypedValueStatement is a Statement that also has Value and
    /// ValueParam properties but not Type and the Value property
    /// is the default content.
    /// </summary>
    [ContentProperty("Value")]
    public abstract class UntypedValueStatement : Statement
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(UntypedValueStatement), null);

        public string Path { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty);
        }
    }
}
