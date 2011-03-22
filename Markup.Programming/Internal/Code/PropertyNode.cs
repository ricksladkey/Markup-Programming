namespace Markup.Programming.Core
{
    public class PropertyNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public string PropertyName { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return PathHelper.GetProperty(engine, Context.Evaluate(engine), PropertyName);
        }
        protected override void OnSet(Engine engine, object value)
        {
            PathHelper.SetProperty(engine, Context.Evaluate(engine), PropertyName, value);
        }
    }
}
