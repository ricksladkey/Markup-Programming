namespace Markup.Programming.Core
{
    public class WhileNode : FramedStatementNode
    {
        public ExpressionNode Condition { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            while (TypeHelper.ConvertToBool(Condition.Evaluate(engine)))
            {
                Body.Execute(engine);
                engine.ClearShouldContinue();
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
