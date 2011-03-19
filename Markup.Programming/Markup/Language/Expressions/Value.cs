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
    /// The Value expression simply returns Content or Path, optionally
    /// converted to Type.
    /// </summary>
    [ContentProperty("Content")]
    public class Value : TypedExpession
    {
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(Value), null);

        public bool SuppressAttach { get; set; }

        public bool Quote { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!SuppressAttach) Attach(ContentProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            if (Quote) return engine.Quote(ContentProperty);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            return engine.Evaluate(ContentProperty, Path, PathExpression, type);
        }
    }
}
