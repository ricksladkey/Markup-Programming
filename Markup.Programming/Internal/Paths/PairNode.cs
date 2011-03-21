using System.Collections;

namespace Markup.Programming.Core
{
    public class PairNode : PathNode
    {
        public PathNode Key { get; set; }
        public PathNode Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return new DictionaryEntry(Key.Evaluate(engine, value), Value.Evaluate(engine, value));
        }
    }
}
