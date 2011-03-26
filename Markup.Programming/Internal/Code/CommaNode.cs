namespace Markup.Programming.Core
{
    public class CommaNode : ExpressionNode
    {
        public ExpressionNode Operand1 { get; set; }
        public ExpressionNode Operand2 { get; set; }
        protected override object OnGet(Engine engine)
        {
            Operand1.Get(engine);
            return Operand2.Get(engine);
        }
    }
}
