using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class CollectionInitializerNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public ExpressionNode Collection { get; set; }
        public IList<ExpressionNode> Items { get; set; }
        protected override object OnGet(Engine engine)
        {
            var collection = Collection.Get(engine) as IList;
            foreach (var item in Items) collection.Add(item.Get(engine));
            return Context == Collection ? collection : Context.Get(engine);
        }
    }
}
