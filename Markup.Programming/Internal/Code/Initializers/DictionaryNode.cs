using System;
using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class DictionaryNode : ExpressionNode
    {
        public ExpressionNode Type { get; set; }
        public IList<ExpressionNode> Items { get; set; }
        protected override object OnGet(Engine engine)
        {
            var context = Type != null ?
                TypeHelper.CreateInstance(Type.Get(engine) as Type) as IDictionary : new Dictionary<object, object>();
            foreach (var item in Items)
            {
                var entry = (DictionaryEntry)item.Get(engine);
                context.Add(entry.Key, entry.Value);
            }
            return context;
        }
    }
}
