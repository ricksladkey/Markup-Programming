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
    /// The Value expression simply returns Val or Path, optionally
    /// converted to Type.
    /// </summary>
    [ContentProperty("Val")]
    public class Value : TypedExpession
    {
        public object Val
        {
            get { return (object)GetValue(ValProperty); }
            set { SetValue(ValProperty, value); }
        }

        public static readonly DependencyProperty ValProperty =
            DependencyProperty.Register("Val", typeof(object), typeof(Value), null);

        public bool SuppressAttach { get; set; }

        public bool Quote { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!SuppressAttach) Attach(ValProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            if (Quote) return engine.Quote(ValProperty);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            return engine.Evaluate(ValProperty, Path, PathExpression, type);
        }
    }
}
