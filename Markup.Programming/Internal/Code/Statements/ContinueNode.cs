namespace Markup.Programming.Core
{
    public class ContinueNode : StatementNode
    {
        protected override void OnExecute(Engine engine) { engine.SetShouldContinue(); }
    }
}
