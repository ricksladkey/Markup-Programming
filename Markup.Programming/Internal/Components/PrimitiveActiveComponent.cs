using System.Windows.Markup;
using System.Windows;
namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    [ContentProperty("Body")]
    public abstract class PrimitiveActiveComponent : Statement
    {
        public PrimitiveActiveComponent()
        {
            Body = new StatementCollection();
        }

        public StatementCollection Body
        {
            get { return (StatementCollection)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(StatementCollection), typeof(PrimitiveActiveComponent), null);

        /// <summary>
        /// Compatbility API.
        /// </summary>
        public StatementCollection Actions { get { return Body; } }

        protected override void OnAttached()
        {
            foreach (var component in Body) component.Attach(AssociatedObject);
        }

        protected virtual void ExecuteBody(Engine engine)
        {
            Body.Execute(engine);
        }
    }
#else
    using System.Windows;
    using System.Windows.Interactivity; // portable
    public abstract class PrimitiveActiveComponent : TriggerBase<DependencyObject>, IStatement
    {
        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public string ContextPath { get; set; }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(Handler), null);

        private PathExpression contextPathExpression = new PathExpression();
        protected PathExpression ContextPathExpression { get { return contextPathExpression; } }

        protected void Attach(params DependencyProperty[] properties)
        {
            ExecutionHelper.Attach(this, properties);
        }

        protected virtual void ExecuteBody(Engine engine)
        {
            InvokeActions(engine.EventArgs);
        }
        
        public void Execute(Engine engine)
        {
            Process(engine);
        }

        public object Process(Engine engine)
        {
            return engine.With(this, e => OnProcess(engine));
        }

        protected abstract object OnProcess(Engine engine);

        protected abstract void OnExecute(Engine engine);
    }
#endif
}
