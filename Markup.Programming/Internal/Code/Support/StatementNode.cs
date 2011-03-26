namespace Markup.Programming.Core
{
    public abstract class StatementNode : Node
    {
        public void Execute(Engine engine) { OnExecute(engine); }
        protected abstract void OnExecute(Engine engine);
    }
}
