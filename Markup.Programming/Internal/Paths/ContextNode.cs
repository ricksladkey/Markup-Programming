namespace Markup.Programming.Core
{
    public class ContextNode : ExpressionNode
    {
        protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
    }
}
