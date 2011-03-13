using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class EventHandler : Handler
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(EventHandler), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            new Engine().With(this, engine => RegisterHandler(engine, Path));
        }

        protected override void OnEventHandler(Engine engine)
        {
            ExecuteBody(engine);
        }
    }
}
