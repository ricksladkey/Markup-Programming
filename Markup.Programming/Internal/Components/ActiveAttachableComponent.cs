#if !INTERACTIVITY

using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    [ContentProperty("Body")]
    public class ActiveAttachableComponent : AttachableComponent
    {
        public ActiveAttachableComponent()
        {
            Body = new StatementCollection();
        }

        public StatementCollection Body
        {
            get { return (StatementCollection)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public StatementCollection Actions { get { return Body; } }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(StatementCollection), typeof(ActiveAttachableComponent), null);

        protected override void OnAttached()
        {
            foreach (var component in Body) component.Attach(AssociatedObject);
        }

        protected void ExecuteBody(object sender, object e)
        {
            new Engine(sender, e).With(this, engine => Body.Execute(engine));
        }
    }
}

#endif
