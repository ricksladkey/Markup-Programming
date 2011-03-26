namespace Markup.Programming.Core
{
    public class BlockNode : FramedExpressionNode
    {
        public StatementNode Body { get; set; }
        protected override object OnFrameGet(Engine engine)
        {
            engine.SetReturnFrame();
            Body.Execute(engine);
            return engine.GetAndResetReturnValue();
        }
    }
}
