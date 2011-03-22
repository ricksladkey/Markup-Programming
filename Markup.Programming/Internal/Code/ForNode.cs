namespace Markup.Programming.Core
{
    public class ForNode : FrameNode
    {
        public StatementNode Initial { get; set; }
        public ExpressionNode Condition { get; set; }
        public ExpressionNode Next { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            Initial.Execute(engine);
            while (TypeHelper.ConvertToBool(Condition.Evaluate(engine)))
            {
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
                Next.Evaluate(engine);
            }
        }
    }
}
