namespace Markup.Programming.Core
{
    public class ContextNode : ExpressionNode
    {
        protected override object OnEvaluate(Engine engine) { return engine.Context; }
    }
}
