namespace Markup.Programming.Core
{
    public class ItemNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public ExpressionNode Index { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return PathHelper.GetItem(engine, Context.Evaluate(engine), Index.Evaluate(engine));
        }
        protected override void OnSet(Engine engine, object value)
        {
            PathHelper.SetItem(engine, Context.Evaluate(engine), Index.Evaluate(engine), value);
        }
    }
}
