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

        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register("ValuePath", typeof(string), typeof(SetItem), null);

        protected override object OnEvaluate(Engine engine)
        {
            return Set(engine, engine.Evaluate(ValueProperty, ValuePath));
        }
    }
}
