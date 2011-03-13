using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A StackFrame is allocated for each nesting level in the
    /// Markup.Programming execution engine.  It is small
    /// and should remain that way.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Caller = {Caller}, AssociatedObject = {Caller.AssociatedObject}")]
#endif
    public class StackFrame
    {
        [Flags]
        public enum FrameFlags
        {
            Empty = 0x0, // an normal stack frame
            Break = 0x1, // a break frame is one that a break will break to
            Return = 0x2, // a return frame is one that return will return to
            Scope = 0x4, // a scope frame blocks lexical name lookups, e.g. a call
        }

        public IComponent Caller { get; set; }
        public FrameFlags Flags { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public IList<object> YieldedValues { get; set; }

        public bool BreakFrame
        {
            get { return (Flags & FrameFlags.Break) == FrameFlags.Break; }
            set { Flags |= FrameFlags.Break; }
        }

        public bool ReturnFrame
        {
            get { return (Flags & FrameFlags.Return) == FrameFlags.Return; }
            set { Flags |= FrameFlags.Return; }
        }

        public bool ScopeFrame
        {
            get { return (Flags & FrameFlags.Scope) == FrameFlags.Scope; }
            set { Flags |= FrameFlags.Scope; }
        }
    }
}
