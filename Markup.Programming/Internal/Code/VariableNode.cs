namespace Markup.Programming.Core
{
    public class VariableNode : ExpressionNode
    {
        public string VariableName { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return engine.LookupVariable(VariableName);
            return engine.DefineVariableInParentScope(VariableName, value);
        }
    }
}
