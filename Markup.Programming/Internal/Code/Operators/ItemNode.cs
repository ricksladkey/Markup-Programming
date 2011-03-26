namespace Markup.Programming.Core
{
    public class ItemNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public ExpressionNode Index { get; set; }
        protected override object OnGet(Engine engine)
        {
            return PathHelper.GetItem(engine, Context.Get(engine), Index.Get(engine));
        }
        protected override void OnSet(Engine engine, object value)
        {
            PathHelper.SetItem(engine, Context.Get(engine), Index.Get(engine), value);
        }
    }
}
