namespace Markup.Programming.Core
{
    public class BreakNode : StatementNode
    {
        protected override void OnExecute(Engine engine) { engine.SetShouldBreak(); }
    }
}
