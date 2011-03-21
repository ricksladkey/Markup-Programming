namespace Markup.Programming.Core
{
    public class SetNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public ExpressionNode RValue { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return LValue.Evaluate(engine, RValue.Evaluate(engine, value));
        }
    }
}
