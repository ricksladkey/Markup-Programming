using System.Collections;

namespace Markup.Programming.Core
{
    public class PairNode : ExpressionNode
    {
        public ExpressionNode Key { get; set; }
        public ExpressionNode Value { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return new DictionaryEntry(Key.Evaluate(engine), Value.Evaluate(engine));
        }
    }
}
