namespace Markup.Programming.Core
{
    public class CommaNode : ExpressionNode
    {
        public ExpressionNode Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            Context.Evaluate(engine, value);
            return Value.Evaluate(engine, value);
        }
    }
}
