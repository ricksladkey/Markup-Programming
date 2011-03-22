namespace Markup.Programming.Core
{
    public class BlockNode : FramedExpressionNode
    {
        protected override object OnEvaluateFrame(Engine engine)
        {
            engine.SetReturnFrame();
            Body.Execute(engine);
            return engine.GetAndResetReturnValue();
        }
    }
}
