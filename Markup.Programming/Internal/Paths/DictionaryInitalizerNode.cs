using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class DictionaryInitializerNode : PathNode
    {
        public PathNode Dictionary { get; set; }
        public IList<PathNode> Items { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            var dictionary = Dictionary.Evaluate(engine, value) as IDictionary;
            foreach (var item in Items)
            {
                var entry = (DictionaryEntry)item.Evaluate(engine, value);
                dictionary.Add(entry.Key, entry.Value);
            }
            return Context == Dictionary ? dictionary : Context.Evaluate(engine, value);
        }
    }
}
