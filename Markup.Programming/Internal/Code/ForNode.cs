namespace Markup.Programming.Core
{
    public class ForNode : FramedStatementNode
    {
        public StatementNode Initial { get; set; }
        public ExpressionNode Condition { get; set; }
        public ExpressionNode Next { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            Initial.Execute(engine);
            while (Condition != null ? TypeHelper.ConvertToBool(Condition.Evaluate(engine)) : true)
            {
                Body.Execute(engine);
                engine.ClearShouldContinue();
                if (engine.ShouldInterrupt) break;
                if (Next != null) Next.Evaluate(engine);
            }
        }
    }
}
