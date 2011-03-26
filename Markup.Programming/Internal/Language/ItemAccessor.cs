using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    [ContentProperty("Arguments")]
    public abstract class ItemAccessor : ArgumentsExpressionWithType
    {
        public object Index
        {
            get { return (object)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(object), typeof(ItemAccessor), null);

        public string IndexPath { get; set; }

        private CodeTree indexCodeTree = new CodeTree();
        protected CodeTree IndexCodeTree { get { return indexCodeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(IndexProperty);
        }

        protected object GetItem(Engine engine)
        {
            return Evaluate(false, engine, null);
        }

        protected object SetItem(Engine engine, object value)
        {
            return Evaluate(true, engine, value);
        }

        private object Evaluate(bool isSet, Engine engine, object value)
        {
            var op = isSet ? Op.SetItem : Op.GetItem;
            var context = engine.GetContext(Path, CodeTree);
            if (Arguments.Count != 0)
            {
                var combinedArgs = new object[] { context }.Concat(Arguments.Evaluate(engine));
                if (isSet) combinedArgs.Concat(new object[] { value });
                return engine.Evaluate(op, combinedArgs.ToArray());
            }
            var type = engine.EvaluateType(TypeProperty, TypePath, TypeCodeTree);
            var index = engine.Evaluate(IndexProperty, IndexPath, IndexCodeTree, type);
            if (!isSet) return engine.Evaluate(op, context, index);
            return engine.Evaluate(op, context, index, value);
        }
    }
}
