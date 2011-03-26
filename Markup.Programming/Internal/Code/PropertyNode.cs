namespace Markup.Programming.Core
{
    public class PropertyNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public string PropertyName { get; set; }
        protected override object OnGet(Engine engine)
        {
            return PathHelper.GetProperty(engine, Context.Get(engine), PropertyName);
        }
        protected override void OnSet(Engine engine, object value)
        {
            PathHelper.SetProperty(engine, Context.Get(engine), PropertyName, value);
        }
    }
}
