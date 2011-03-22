namespace Markup.Programming.Core
{
    public class CommaNode : ExpressionNode
    {
        public ExpressionNode Operand1 { get; set; }
        public ExpressionNode Operand2 { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            Operand1.Evaluate(engine);
            return Operand2.Evaluate(engine);
        }
    }
}
