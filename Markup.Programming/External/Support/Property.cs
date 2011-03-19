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
    public class Property : ResourceObjectBase, IHiddenExpression
    {
        public string Prop { get; set; }

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Property), null);

        public string TypeName { get; set; }

        public override object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Property), null);

        public string Path { get; set; }

        private PathExpression pathExpression = new PathExpression();
        protected PathExpression PathExpression { get { return pathExpression; } }

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
            return engine.Evaluate(ValueProperty, Path, PathExpression, type);
        }
    }
}
