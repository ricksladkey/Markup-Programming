namespace Markup.Programming.Core
{
    public class ValueNode : ExpressionNode
    {
        public object Value { get; set; }
        protected override object OnGet(Engine engine) { return Value; }
    }
}
