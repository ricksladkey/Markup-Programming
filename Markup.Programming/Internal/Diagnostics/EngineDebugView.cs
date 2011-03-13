using System.Linq;
using System.Windows;

namespace Markup.Programming.Core
{

#if DEBUG

    public class EngineDebugView
    {
        private Engine engine;

        public EngineDebugView(Engine engine) { this.engine = engine; }

        StackFrame[] BackTrace { get { return engine.StackBackwards.ToArray(); } }

        //DependencyObject[] AssociatedObjects { get { return engine.StackBackwards.Select(frame => frame.Caller.AssociatedObject).ToArray(); } }
    }

#endif

}
