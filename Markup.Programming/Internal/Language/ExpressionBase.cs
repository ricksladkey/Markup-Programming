
namespace Markup.Programming.Core
{
    /// <summary>
    /// An ExpressionBase is an IExpression or simply a Statement
    /// that has an additional method that produces a value.
    /// If the expression is evaluated as a statement, the
    /// expression is evaluated and the value is discarded.
    /// It would be called Expression  that conflicts with core
    /// libraries.  Implementing classes must override OnEvaluate.
    /// </summary>
    public abstract class ExpressionBase : Statement, IExpression
    {
        protected override void OnExecute(Engine engine)
        {
            // Nothing happens.
        }

        public object Evaluate(Engine engine)
        {
            return Process(engine);
        }

        protected override object OnProcess(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath);
            return OnEvaluate(engine);
        }

        protected abstract object OnEvaluate(Engine engine);
    }
}
