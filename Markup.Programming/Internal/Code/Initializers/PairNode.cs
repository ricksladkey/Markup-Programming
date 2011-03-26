using System.Collections;

namespace Markup.Programming.Core
{
    public class PairNode : ExpressionNode
    {
        public ExpressionNode Key { get; set; }
        public ExpressionNode Value { get; set; }
        protected override object OnGet(Engine engine)
        {
            return new DictionaryEntry(Key.Get(engine), Value.Get(engine));
        }
    }
}
