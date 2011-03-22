using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public abstract class ExpressionNode : StatementNode
    {
        protected override void OnExecute(Engine engine) { Evaluate(engine); }
        public object Evaluate(Engine engine)
        {
            engine.Trace(TraceFlags.Path, "Path: {0}: Evaluate", this.GetType().Name);
            var result = OnEvaluate(engine);
            engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
            return result;
        }
        protected abstract object OnEvaluate(Engine engine);
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
