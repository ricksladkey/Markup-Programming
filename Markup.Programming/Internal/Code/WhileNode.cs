namespace Markup.Programming.Core
{
    public class WhileNode : BodyNode
    {
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            while (TypeHelper.ConvertToBool(Context.Evaluate(engine)))
            {
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
