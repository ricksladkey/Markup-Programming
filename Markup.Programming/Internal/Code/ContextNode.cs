namespace Markup.Programming.Core
{
    public class ContextNode : ExpressionNode
    {
        protected override object OnGet(Engine engine) { return engine.Context; }
    }
}
