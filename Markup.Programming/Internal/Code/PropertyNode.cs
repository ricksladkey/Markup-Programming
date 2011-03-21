namespace Markup.Programming.Core
{
    public class PropertyNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public string PropertyName { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return PathHelper.GetProperty(engine, Context.Evaluate(engine, value), PropertyName);
            return PathHelper.SetProperty(engine, Context.Evaluate(engine, value), PropertyName, value);
        }
    }
}
