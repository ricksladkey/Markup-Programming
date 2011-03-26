namespace Markup.Programming.Core
{
    public class VarNode : StatementNode
    {
        public string VariableName { get; set; }
        public ExpressionNode Value { get; set; }
        protected override void OnExecute(Engine engine)
        {
            var value = Value != null ? Value.Get(engine) : null;
            if (engine.ScriptDepth <= 1)
                engine.DefineVariableInParentScope(VariableName, value);
            else
                engine.DefineVariable(VariableName, value);
        }
    }
}
