using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public abstract class ExpressionNode : StatementNode
    {
        protected enum UnsetValue { Value = 0 };
        public bool IsSet { get; set; }
        protected override void OnExecute(Engine engine) { Evaluate(engine, UnsetValue.Value); }
        public object Evaluate(Engine engine) { return Evaluate(engine, UnsetValue.Value); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: {0} {1} {2}", this.GetType().Name, IsSet, Name);
            if (IsSet && value.Equals(UnsetValue.Value)) engine.Throw("value not supplied");
            var result = OnEvaluate(engine, value);
            engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
            return result;
        }
        protected abstract object OnEvaluate(Engine engine, object value);
    }
}
