using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;
using System.ComponentModel;
using System.Windows.Markup;

namespace Markup.Programming
{
    /// <summary>
    /// A Property declares a PropertyName, its Type and its Value.
    /// </summary>
    [ContentProperty("Value")]
    public class Property : ResourceObject, IHiddenExpression
    {
        public string PropertyName
        {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(Property), null);

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Property), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(Property), null);

        public override object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Path { get; set; }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Property), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty);
        }

        public object Process(Engine engine) { return Evaluate(engine); }
        public void Execute(Engine engine) { Evaluate(engine); }

        public object Evaluate(Engine engine)
        {
            return engine.With(this, e => GetPropertyValue(engine));
        }


        private object GetPropertyValue(Engine engine)
        {
            var type = engine.EvaluateType(TypeProperty, TypeName);
            return engine.Evaluate(ValueProperty, Path, type);
        }
    }
}
