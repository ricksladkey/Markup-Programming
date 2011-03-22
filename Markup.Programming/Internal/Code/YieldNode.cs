namespace Markup.Programming.Core
{
    public class YieldNode : StatementNode
    {
        public ExpressionNode Value { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.YieldValue(Value.Evaluate(engine));
        }
    }
}
