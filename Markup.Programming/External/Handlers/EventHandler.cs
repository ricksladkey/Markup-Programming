using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class EventHandler : Handler
    {
        protected override void OnActiveExecute(Engine engine)
        {
            if (Path != null)
            {
                CodeTree.Compile(engine, CodeType.EventExpression, Path);
                var context = CodeTree.GetContext(engine);
                if (context == null) engine.Throw("context cannot be null");
                RegisterHandler(engine, context, CodeTree.GetEvent(engine));
            }
            else
                RegisterHandler(engine, null, null);
        }

        protected override void OnEventHandler(Engine engine)
        {
            if (Path != null && CodeTree.HasHandler)
                CodeTree.Get(engine);
            else
                base.OnEventHandler(engine);
        }
    }
}
