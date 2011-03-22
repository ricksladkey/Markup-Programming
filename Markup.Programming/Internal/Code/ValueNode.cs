namespace Markup.Programming.Core
{
    public class ValueNode : ExpressionNode
    {
        public object Value { get; set; }
        protected override object OnEvaluate(Engine engine) { return Value; }
    }
}
