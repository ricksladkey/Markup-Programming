namespace Markup.Programming.Core
{
    public class ReturnNode : StatementNode
    {
        public ExpressionNode Value { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.SetReturnValue(Value != null ? Value.Evaluate(engine) : null);
        }
    }
}
