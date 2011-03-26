namespace Markup.Programming.Core
{
    public class PropertyInitializerNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public string PropertyName { get; set; }
        public ExpressionNode Value { get; set; }
        protected override object OnGet(Engine engine)
        {
            var context = Context.Get(engine);
            PathHelper.SetProperty(engine, context, PropertyName, Value.Get(engine));
            return context;
        }
    }
}
