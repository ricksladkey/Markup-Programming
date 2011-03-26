namespace Markup.Programming.Core
{
    public class VariableNode : ExpressionNode
    {
        public string VariableName { get; set; }
        protected override object OnGet(Engine engine)
        {
            if (engine.ScriptDepth == 1)
                return engine.GetVariableInParentScope(VariableName);
            else
                return engine.GetVariable(VariableName);
        }
        protected override void OnSet(Engine engine, object value)
        {
            if (engine.ScriptDepth == 1)
                engine.SetVariableInParentScope(VariableName, value);
            else
                engine.SetVariable(VariableName, value);
        }
    }
}
