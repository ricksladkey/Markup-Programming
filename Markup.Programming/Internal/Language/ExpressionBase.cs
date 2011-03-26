
namespace Markup.Programming.Core
{
    /// <summary>
    /// An ExpressionBase is an IExpression or simply a Statement
    /// that has an additional method that produces a value.
    /// If the expression is evaluated as a statement, the
    /// expression is evaluated and the value is discarded.
    /// It would be called Expression  that conflicts with core
    /// libraries.  Implementing classes must override OnGet.
    /// </summary>
    public abstract class ExpressionBase : Statement, IExpression
    {
        public string Path { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

        protected override void OnExecute(Engine engine)
        {
            // Nothing happens.
        }

        public object Get(Engine engine)
        {
            // Inline Process(engine);
            return engine.FrameFunc(this, OnProcess);
        }

        protected override object OnProcess(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath, ContextCodeTree);
            return OnGet(engine);
        }

        protected abstract object OnGet(Engine engine);
    }
}
