namespace Markup.Programming.Core
{
    public class WhileNode : FrameNode
    {
        public ExpressionNode Condition { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            while (TypeHelper.ConvertToBool(Condition.Evaluate(engine)))
            {
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
