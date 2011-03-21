namespace Markup.Programming.Core
{
    public class SetNode : PathNode
    {
        public PathNode LValue { get; set; }
        public PathNode RValue { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return LValue.Evaluate(engine, RValue.Evaluate(engine, value));
        }
    }
}
