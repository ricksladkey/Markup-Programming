namespace Markup.Programming.Core
{
    public class PropertyInitializerNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public string PropertyName { get; set; }
        public ExpressionNode Value { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            var context = Context.Evaluate(engine);
            PathHelper.SetProperty(engine, context, PropertyName, Value.Evaluate(engine));
            return context;
        }
    }
}
