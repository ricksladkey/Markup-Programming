namespace Markup.Programming.Core
{
    public class ConditionalNode : ExpressionNode
    {
        public ExpressionNode Conditional { get; set; }
        public ExpressionNode IfTrue { get; set; }
        public ExpressionNode IfFalse { get; set; }
        protected override object OnGet(Engine engine)
        {
            bool condition = TypeHelper.ConvertToBool(Conditional.Get(engine));
            return condition ? IfTrue.Get(engine) : IfFalse.Get(engine);
        }
    }
}
