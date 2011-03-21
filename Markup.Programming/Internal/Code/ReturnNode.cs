namespace Markup.Programming.Core
{
    public class ReturnNode : ExpressionNode
    {
        public ExpressionNode Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            engine.SetReturnValue(Value.Evaluate(engine, value));
            return null;
        }
    }
}
