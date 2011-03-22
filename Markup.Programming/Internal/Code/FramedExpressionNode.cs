namespace Markup.Programming.Core
{
    public abstract class FramedExpressionNode : ExpressionNode
    {
        public StatementNode Body { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return engine.With(this, e => OnEvaluateFrame(engine));
        }
        protected abstract object OnEvaluateFrame(Engine engine);
    }
}
