namespace Markup.Programming.Core
{
    public abstract class StatementNode : PathNode
    {
        public ExpressionNode Context { get; set; }
        public string Name { get; set; }
        public override object Process(Engine engine) { Execute(engine); return null; }
        public void Execute(Engine engine) { OnExecute(engine); }
        protected abstract void OnExecute(Engine engine);
    }
}
