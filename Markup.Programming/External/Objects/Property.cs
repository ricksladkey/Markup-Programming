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
    public class Property : ResourceComponent, IHiddenExpression
    {
        public string Prop { get; set; }

        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(Property), null);

        public string TypePath { get; set; }

        private CodeTree typeCodeTree = new CodeTree();
        protected CodeTree TypeCodeTree { get { return typeCodeTree; } }

        public override object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Property), null);

        public string Path { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty, ValueProperty);
        }

        public object Process(Engine engine) { return Evaluate(engine); }
        public void Execute(Engine engine) { Evaluate(engine); }

        public Type GetType(Engine engine)
        {
            return engine.With(this, e => engine.EvaluateType(TypeProperty, TypePath, TypeCodeTree));
        }

        public object Evaluate(Engine engine)
        {
            return engine.With(this, e => engine.Evaluate(ValueProperty, Path, CodeTree));
        }
    }
}
