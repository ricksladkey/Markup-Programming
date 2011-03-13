using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace Markup.Programming.Core
{

#if DEBUG

    public class EngineDebugView
    {
        private Engine engine;

        public EngineDebugView(Engine engine) { this.engine = engine; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        StackFrame[] BackTrace { get { return engine.StackBackwards.ToArray(); } }
    }

#endif

}
