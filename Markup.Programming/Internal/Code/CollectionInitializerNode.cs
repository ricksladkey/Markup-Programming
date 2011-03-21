using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class CollectionInitializerNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public ExpressionNode Collection { get; set; }
        public IList<ExpressionNode> Items { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            var collection = Collection.Evaluate(engine, value) as IList;
            foreach (var item in Items) collection.Add(item.Evaluate(engine, value));
            return Context == Collection ? collection : Context.Evaluate(engine, value);
        }
    }
}
