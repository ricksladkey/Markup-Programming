using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class EventHandler : Handler
    {
        public string Path { get; set; }

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
