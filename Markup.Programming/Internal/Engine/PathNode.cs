using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Markup.Programming.Core
{
    public abstract class PathNode
    {
        public bool IsGet { get; set; }
        public PathNode Context { get; set; }
        public void ThrowIfUnset(object value) { if (PathExpression.IsUnset(value)) ThrowHelper.Throw("unset"); }
        public object Evaluate(Engine engine, object value)
        {
            engine.Trace(TraceFlags.Path, "Path: evaluate {0}", this.GetType().Name);
            var result = OnEvaluate(engine, value);
            engine.Trace(TraceFlags.Path, "Path: {0} = {1}", this.GetType().Name, result);
            return result;
        }
        protected abstract object OnEvaluate(Engine engine, object value);
    }

    public class ValueNode : PathNode
    {
        public object Value { get; set; }
        protected override object OnEvaluate(Engine engine, object value) { return Value; }
    }

    public class OpNode : PathNode
    {
        public OpNode() { Operands = new List<PathNode>(); }
        public Operator Operator { get; set; }
        public IList<PathNode> Operands { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return engine.Evaluate(Operator, Operands.Select(operand => operand.Evaluate(engine, value)).ToArray());
        }
    }

    public class ContextNode : PathNode
    {
        protected override object OnEvaluate(Engine engine, object value) { return engine.Context; }
    }

    public class ParameterNode : PathNode
    {
        public string ParameterName { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (IsGet) return engine.LookupParameter(ParameterName);
            ThrowIfUnset(value);
            engine.DefineParameterInParentScope(ParameterName, value);
            return value;
        }
    }

    public class PropertyNode : PathNode
    {
        public string PropertyName { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (IsGet) return PathHelper.GetProperty(Context.Evaluate(engine, value), PropertyName);
            ThrowIfUnset(value);
            PathHelper.SetProperty(Context.Evaluate(engine, value), PropertyName, value);
            return value;
        }
    }

    public class ItemNode : PathNode
    {
        public PathNode Index { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (IsGet) return PathHelper.GetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value));
            ThrowIfUnset(value);
            PathHelper.SetItem(Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
            return value;
        }
    }

    public class MethodNode : PathNode
    {
        private object setValue;
        public string MethodName { get; set; }
        public IList<PathNode> Arguments { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            setValue = value;
            return Call(engine, Arguments.Select(argument => argument.Evaluate(engine, value)).ToArray());
        }
        public virtual object Call(Engine engine, object[] args)
        {
            var context = Context.Evaluate(engine, setValue);
            return MethodHelper.CallMethod(MethodName, false, context.GetType(), context, args, null, engine);
        }
    }

    public class StaticMethodNode : MethodNode
    {
        public Type Type { get; set; }
        public override object Call(Engine engine, object[] args)
        {
            return MethodHelper.CallMethod(MethodName, true, Type, null, args, null, engine);
        }
    }
}
