using System.Collections;

namespace Markup.Programming.Core
{
    public class PairNode : ExpressionNode
    {
        public ExpressionNode Key { get; set; }
        public ExpressionNode Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return new DictionaryEntry(Key.Evaluate(engine, value), Value.Evaluate(engine, value));
        }
    }
}
