using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    [ContentProperty("Arguments")]
    public abstract class Accessor : ArgumentsExpressionWithType
    {
        public object Index
        {
            get { return (object)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public string IndexPath { get; set; }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(object), typeof(Accessor), null);

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Accessor), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(IndexProperty);
        }

        protected object Get(Engine engine)
        {
            return Evaluate(true, engine, null);
        }

        protected object Set(Engine engine, object value)
        {
            return Evaluate(false, engine, value);
        }

        private object Evaluate(bool get, Engine engine, object value)
        {
            var op = get ? Operator.GetItem : Operator.SetItem;
            var context = engine.GetContext(Path);
            if (Arguments.Count != 0)
            {
                var combinedArgs = new object[] { context }.Concat(Arguments.Evaluate(engine));
                if (!get) combinedArgs.Concat(new object[] { value });
                return engine.Evaluate(op, combinedArgs.ToArray());
            }
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var index = engine.Evaluate(IndexProperty, IndexPath, type);
            if (get) return engine.Evaluate(op, context, index);
            return engine.Evaluate(op, context, index, value);
        }
    }
}
