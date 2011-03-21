namespace Markup.Programming.Core
{
    public class PropertyInitializerNode : PathNode
    {
        public PathNode Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            var context = Context.Evaluate(engine, value);
            PathHelper.SetProperty(engine, context, Name, Value.Evaluate(engine, value));
            return context;
        }
    }
}
