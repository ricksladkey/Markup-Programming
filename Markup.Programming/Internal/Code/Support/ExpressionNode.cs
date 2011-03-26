using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public abstract class ExpressionNode : StatementNode
    {
        protected override void OnExecute(Engine engine) { Get(engine); }
        public object Get(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: {0}: Get", this.GetType().Name);
            var result = OnGet(engine);
            engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
            return result;
        }
        protected abstract object OnGet(Engine engine);
        public object Set(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: {0}: Set {1}", this.GetType().Name, value);
            OnSet(engine, value);
            return value;
        }
        protected virtual void OnSet(Engine engine, object value)
        {
            engine.Throw("set not supported");
        }
    }
}
