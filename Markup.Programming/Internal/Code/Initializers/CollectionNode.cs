using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class CollectionNode : ExpressionNode
    {
        public ExpressionNode Type { get; set; }
        public IList<ExpressionNode> Items { get; set; }
        protected override object OnGet(Engine engine)
        {
            if (Type != null)
            {
                var context = TypeHelper.CreateInstance(Type.Get(engine) as Type) as IList;
                foreach (var item in Items) context.Add(item.Get(engine));
                return context;
            }
            return TypeHelper.CreateCollection(Items.Select(item => item.Get(engine)).ToArray());
        }
    }
}
