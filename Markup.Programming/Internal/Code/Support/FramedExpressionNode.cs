namespace Markup.Programming.Core
{
    public abstract class FramedExpressionNode : ExpressionNode
    {
        protected override object OnGet(Engine engine)
        {
            return engine.FrameFunc(this, OnFrameGet);
        }
        protected abstract object OnFrameGet(Engine engine);
    }
}
