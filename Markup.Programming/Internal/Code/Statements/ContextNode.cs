namespace Markup.Programming.Core
{
    public class ContexteNode : FramedStatementNode
    {
        public ExpressionNode Context { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.DefineVariable(Engine.ContextKey, Context.Get(engine));
            Body.Execute(engine);
        }
    }
}
