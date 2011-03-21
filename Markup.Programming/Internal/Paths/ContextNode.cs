namespace Markup.Programming.Core
{
    public class ContextNode : PathNode
    {
        protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
    }
}
