using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class SetItem : Accessor
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SetItem), null);

        public string ValuePath { get; set; }

        private PathExpression valuePathExpression = new PathExpression();
        protected PathExpression ValuePathExpression { get { return valuePathExpression; } }

        protected override object OnEvaluate(Engine engine)
        {
            return Set(engine, engine.Evaluate(ValueProperty, ValuePathExpression, ValuePath));
        }
    }
}
