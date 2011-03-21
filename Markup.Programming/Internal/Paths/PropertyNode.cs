namespace Markup.Programming.Core
{
    public class PropertyNode : PathNode
    {
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return PathHelper.GetProperty(engine, Context.Evaluate(engine, value), Name);
            return PathHelper.SetProperty(engine, Context.Evaluate(engine, value), Name, value);
        }
    }
}
