namespace Markup.Programming.Core
{
    public class ReturnNode : PathNode
    {
        protected override object OnEvaluate(Engine engine, object value)
        {
            engine.SetReturnValue(Context.Evaluate(engine, value));
            return null;
        }
    }
}
