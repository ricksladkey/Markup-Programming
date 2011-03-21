namespace Markup.Programming.Core
{
    public class ConditionalNode : ExpressionNode
    {
        public ExpressionNode IfTrue { get; set; }
        public ExpressionNode IfFalse { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            bool condition = TypeHelper.ConvertToBool(Context.Evaluate(engine, value));
            return condition ? IfTrue.Evaluate(engine, value) : IfFalse.Evaluate(engine, value);
        }
    }
}
