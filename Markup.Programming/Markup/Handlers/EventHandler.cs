using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class EventHandler : Handler
    {
        protected override void OnActiveExecute(Engine engine)
        {
            RegisterHandler(engine, Path);
        }

        protected override void OnEventHandler(Engine engine)
        {
            ExecuteBody(engine);
        }
    }
}
