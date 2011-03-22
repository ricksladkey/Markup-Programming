namespace Markup.Programming.Core
{
    public class VarNode : StatementNode
    {
        public string VariableName { get; set; }
        public ExpressionNode Value { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.DefineVariable(VariableName, Value.Evaluate(engine));
        }
    }
}
