using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Eval expression simply returns Value or Path, optionally
    /// converted to Type.
    /// </summary>
    [ContentProperty("Value")]
    public class Eval : TypedExpession
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Eval), null);

        public bool SuppressAttach { get; set; }

        public bool Quote { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!SuppressAttach) Attach(ValueProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            if (Quote) return engine.Quote(ValueProperty);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            return engine.Evaluate(ValueProperty, Path, PathExpression, type);
        }
    }
}
