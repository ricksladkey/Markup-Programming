namespace Markup.Programming.Core
{
    public class VariableNode : ExpressionNode
    {
        public string VariableName { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return engine.GetVariable(VariableName);
        }
        protected override void OnSet(Engine engine, object value)
        {
            engine.SetVariable(VariableName, value);
        }
    }
}
