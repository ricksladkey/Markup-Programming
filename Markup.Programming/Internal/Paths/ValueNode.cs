namespace Markup.Programming.Core
{
    public class ValueNode : PathNode
    {
        public object Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value) { return Value; }
    }
}
