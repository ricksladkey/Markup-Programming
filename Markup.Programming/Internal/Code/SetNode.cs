namespace Markup.Programming.Core
{
    public class SetNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public ExpressionNode RValue { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return LValue.Set(engine, RValue.Evaluate(engine));
        }
    }
}
