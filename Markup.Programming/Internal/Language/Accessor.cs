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

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(object), typeof(Accessor), null);

        public string IndexPath { get; set; }

        private PathExpression indexPathExpression = new PathExpression();
        protected PathExpression IndexPathExpression { get { return indexPathExpression; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(IndexProperty);
        }

        protected object Get(Engine engine)
        {
            return Evaluate(false, engine, null);
        }

        protected object Set(Engine engine, object value)
        {
            return Evaluate(true, engine, value);
        }

        private object Evaluate(bool isSet, Engine engine, object value)
        {
            var op = isSet ? Operator.SetItem : Operator.GetItem;
            var context = engine.GetContext(PathExpression, Path);
            if (Arguments.Count != 0)
            {
                var combinedArgs = new object[] { context }.Concat(Arguments.Evaluate(engine));
                if (isSet) combinedArgs.Concat(new object[] { value });
                return engine.Evaluate(op, combinedArgs.ToArray());
            }
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var index = engine.Evaluate(IndexProperty, IndexPathExpression, IndexPath, type);
            if (!isSet) return engine.Evaluate(op, context, index);
            return engine.Evaluate(op, context, index, value);
        }
    }
}
