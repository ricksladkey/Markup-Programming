using System.Windows.Markup;
using System.Windows;
namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    [ContentProperty("Body")]
    public abstract class PrimitiveActiveComponent : ComponentBase
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

        protected void ExecuteBody(object sender, object e)
        {
            new Engine(sender, e).With(this, engine => Body.Execute(engine));
        }

        protected void ExecuteBody(Engine engine)
        {
            Body.Execute(engine);
        }
    }
#else
    using System.Windows;
    using System.Windows.Interactivity; // portable
    public class PrimitiveActiveComponent : TriggerBase<DependencyObject>, IComponent
    {
        protected void ExecuteBody(object sender, object e)
        {
            InvokeActions(e);
        }

        protected void ExecuteBody(Engine engine)
        {
            InvokeActions(engine.EventArgs);
        }
    }
#endif
}
