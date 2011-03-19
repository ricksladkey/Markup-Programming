using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Markup.Programming.Core
{
    [Flags]
    public enum TraceFlags
    {
        Console = 0x1,
        Stack = 0x2,
        Operator = 0x4,
        Variable = 0x8,
        Call = 0x10,
        Path = 0x20,
        Events = 0x40,
        All = 0xff,
    }

    /// <summary>
    /// An Engine object is the heart of Markup.Programmings's
    /// execution engine.  It is created as needed, lives through
    /// the course of an execution and disappears when control
    /// returns to the system.  It contains the stack itself and information
    /// associated with names, scopes, control flow and stack frames.
    /// It is lightweight enough to be created and discarded at will.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Frames = {stack.Count}"), DebuggerTypeProxy(typeof(EngineDebugView))]
#endif
    public class Engine
    {
        public static string ContextKey = "@";
        public static string AssociatedObjectKey = "@AssociatedObject";
        public static string SenderKey = "@Sender";
        public static string EventArgsKey = "@EventArgs";

        private static int nextId = 0;
        private int id;
        private TraceFlags TraceFilter = Configuration.TraceDefaults;
        private bool shouldBreak;
        private bool shouldReturn;
        private object returnValue;
        private List<StackFrame> stack = new List<StackFrame>();
        private IDictionary<string, Function> functions;
        public StackFrame CurrentFrame { get { return stack.Count >= 1 ? stack[stack.Count - 1] : null; } }
        private StackFrame ParentFrame { get { return stack.Count >= 2 ? stack[stack.Count - 2] : null; } }
        private string FrameInfo { get { return string.Format("{0}/{1}", id, stack.Count); } }

        private BuiltinImplementor builtinImplementor;
        private BuiltinImplementor BuiltinImplementor
        {
            get
            {
                if (builtinImplementor != null) return builtinImplementor;
                builtinImplementor = new BuiltinImplementor(this);
                return builtinImplementor;
            }
        }

        public Engine()
            : this(null, null)
        {
        }

        public Engine(object parameter)
            : this(null, parameter)
        {
        }

        public Engine(object sender, object eventArgs)
        {
            id = nextId++;
            Sender = sender;
            EventArgs = eventArgs;
        }

        public object Sender { get; private set; }
        public object EventArgs { get; private set; }
        public bool ShouldInterrupt { get { return shouldBreak || shouldReturn; } }
        public IEnumerable<StackFrame> Stack { get { return stack; } }
        public IEnumerable<StackFrame> StackBackwards
        {
            get { for (int i = stack.Count - 1; i >= 0; i--) yield return stack[i]; }
        }

        public string GetName(IComponent component)
        {
            return "p:" + component.GetType().Name;
        }

        private void PushFrame(IComponent caller)
        {
            stack.Add(new StackFrame(this, caller));
            TraceStack("PushFrame: {0}", caller.GetType().Name);
        }

        private void PopFrame()
        {
            var frame = CurrentFrame;
            if (frame.BreakFrame && shouldBreak) shouldBreak = false;
            if (frame.ReturnFrame && shouldReturn)
            {
                shouldReturn = false;
            }
            TraceStack("PopFrame: {0}", frame.Caller.GetType().Name);
            stack.RemoveAt(stack.Count - 1);
        }

        public void With(IComponent caller, Action<Engine> action)
        {
            PushFrame(caller);
            action(this);
            PopFrame();
        }

        public void With(IComponent caller, IDictionary<string, object> variables, Action<Engine> action)
        {
            PushFrame(caller);
            foreach (var pair in variables) DefineVariable(pair.Key, pair.Value, false, true);
            action(this);
            PopFrame();
        }

        public TResult With<TResult>(IComponent caller, Func<Engine, TResult> func)
        {
            PushFrame(caller);
            var result = func(this);
            PopFrame();
            return result;
        }

        public TResult With<TResult>(IComponent caller, IDictionary<string, object> variables, Func<Engine, TResult> func)
        {
            PushFrame(caller);
            foreach (var pair in variables) DefineVariable(pair.Key, pair.Value, false, true);
            var result = func(this);
            PopFrame();
            return result;
        }

        public void SetBreakFrame()
        {
            CurrentFrame.BreakFrame = true;
        }

        public void SetReturnFrame()
        {
            CurrentFrame.ReturnFrame = true;
            returnValue = null;
        }

        public void SetScopeFrame()
        {
            CurrentFrame.ScopeFrame = true;
        }

        public void SetShouldBreak()
        {
            shouldBreak = true;
        }

        public void SetReturnValue(object value)
        {
            TraceStack("Return: {0}", value);
            returnValue = value;
            shouldReturn = true;
        }

        public object GetAndResetReturnValue()
        {
            var result = returnValue;
            returnValue = null;
            return result;
        }

        public void SetYieldFrame()
        {
            CurrentFrame.ReturnFrame = true;
            CurrentFrame.BreakFrame = true;
            CurrentFrame.YieldedValues = new List<object>();
        }

        public void YieldValue(object value)
        {
            TraceStack("Yield: {0}", value);
            foreach (var frame in StackBackwards)
            {
                if (frame.ReturnFrame)
                {
                    frame.YieldedValues.Add(value);
                    return;
                }
            }
            Throw("missing yield frame");
        }

        public IList<object> GetYieldedValues()
        {
            if (CurrentFrame.YieldedValues == null) Throw("missing yielded values");
            return CurrentFrame.YieldedValues;
        }

        public void DefineVariable(string name, object value)
        {
            Trace(TraceFlags.Variable, "Define: {0} = {1}", name, value);
            DefineVariable(name, value, false, false);
        }

        public object DefineVariableInParentScope(string name, object value)
        {
            DefineVariable(name, value, true, false);
            return value;
        }

        private void DefineVariable(string name, object value, bool parentFrame, bool noError)
        {
            if (!noError && !PathExpression.IsValidIdentifier(name)) Throw("invalid identifier: " + name);
            Trace(TraceFlags.Variable, "DefineVariable: {0} = {1}", name, value);
            var frame = parentFrame ? (ParentFrame ?? CurrentFrame) : CurrentFrame;
            if (frame == null) Throw("no frame for variable: " + name);
            if (frame.Variables == null) frame.Variables = new NameDictionary();
            frame.Variables[name] = value;
        }

        public IDictionary<string, object> GetClosure()
        {
            var closure = new Dictionary<string, object>();
            foreach (var frame in StackBackwards)
            {
                if (frame.Variables == null) continue;
                    foreach (var name in frame.Variables.Keys)
                        if (!closure.ContainsKey(name)) closure.Add(name, frame.Variables[name]);
                if (frame.ScopeFrame) break;
            }
            if (!closure.ContainsKey(AssociatedObjectKey)) closure.Add(AssociatedObjectKey, CurrentFrame.Caller.AssociatedObject);
            return closure;
        }

        public bool TryLookupVariable(string name, out object value)
        {
            foreach (var frame in StackBackwards)
            {
                if (frame.Variables != null && frame.Variables.ContainsKey(name))
                {
                    value = frame.Variables[name];
                    return true;
                }
                if (frame.ScopeFrame) break;
            }
            return BuiltinImplementor.TryLookupVariable(name, out value);
        }

        public object LookupVariable(string name)
        {
            var value = null as object;
            if (TryLookupVariable(name, out value))
            {
                Trace(TraceFlags.Variable, "Lookup: {0} = {1}", name, value);
                return value;
            }
            return Throw("variable not found: " + name);
        }

        public void DefineFunction(string name, Function value)
        {
            if (functions == null) functions = new Dictionary<string, Function>();
            if (functions.ContainsKey(name) && functions[name] != value)
                Throw("function redefinition: " + name);
            functions[name] = value;
        }

        public bool TryLookupFunction(string name, out Function value)
        {
            if (functions != null && functions.ContainsKey(name))
            {
                value = functions[name];
                return true;
            }
            value = null;
            return false;
        }

        public Function LookupFunction(string name)
        {
            var value = null as Function;
            if (TryLookupFunction(name, out value)) return value;
            return Throw("function not found: " + name) as Function;
        }

        public Type LookupType(string name)
        {
            if (name.Split(',').Length > 2) return TypeHelper.ConvertToType(name);
            return TypeHelper.ResolvePartialType(name);
        }

        public object Context
        {
            get
            {
                var value = null as object;
                if (TryLookupVariable(ContextKey, out value)) return value;
                return null;
            }
        }

        public object GetContext(string path, PathExpression pathExpression)
        {
            return path != null ? GetPath(path, pathExpression) : Context;
        }

        public void SetContext(DependencyProperty property, string path, PathExpression pathExpression)
        {
            if (HasBindingOrValue(property, path))
                SetContext(Evaluate(property, path, pathExpression));
        }

        public void SetContext(object context)
        {
            Trace(TraceFlags.Variable, "Setting context = {0}", context);
            DefineVariable(Engine.ContextKey, context, false, true);
        }

        public bool HasBindingOrValue(DependencyProperty property, string path)
        {
            var caller = CurrentFrame.Caller as DependencyObject;
            return PathHelper.HasBindingOrValue(caller, property, path);
        }

        public object GetPath(string path, PathExpression pathExpression)
        {
            if (pathExpression == null) pathExpression = new PathExpression();
            return pathExpression.Compile(this, false, false, path).Evaluate(this);
        }

        public object SetPath(string path, PathExpression pathExpression, object value)
        {
            if (pathExpression == null) pathExpression = new PathExpression();
            return pathExpression.Compile(this, true, false, path).Evaluate(this, value);
        }

        public object CallPath(string path, PathExpression pathExpression, IEnumerable<object> args)
        {
            if (pathExpression == null) pathExpression = new PathExpression();
            return pathExpression.Compile(this, false, true, path).Call(this, args);
        }

        public bool ShouldTrace(TraceFlags flags)
        {
            return (flags & TraceFilter) != 0;
        }

        [Conditional("TRACE")]
        public void Trace(TraceFlags flags, string format, params object[] args)
        {
            if (ShouldTrace(flags)) TraceHelper.WriteLine(format, args);
        }

        [Conditional("TRACE")]
        private void TraceStack(string format, params object[] args)
        {
            Trace(TraceFlags.Stack, "[" + FrameInfo + "]" + format, args);
        }

        public object Throw(string message, params object[] args)
        {
            return ThrowHelper.Throw(new InvalidOperationException(string.Format(message, args)), this);
        }

        public object Evaluate(Op op, params object[] values)
        {
            return OperatorHelper.Evaluate(this, op, ExpressionOrValue.ValueArray(values));
        }

        public object Evaluate(Op op, IEnumerable<IExpression> collection)
        {
            return OperatorHelper.Evaluate(this, op, ExpressionOrValue.ExpressionArray(collection));
        }

        public object Evaluate(Op op, ExpressionOrValue[] expressions)
        {
            return OperatorHelper.Evaluate(this, op, expressions);
        }

        public object Evaluate(Op op, ExpressionCollection collection)
        {
            return OperatorHelper.Evaluate(this, op, ExpressionOrValue.ExpressionArray(collection));
        }

        public object Evaluate(AssignmentOp op, object lhs, object rhs)
        {
            switch (op)
            {
                case AssignmentOp.Assign:
                    return rhs;
                case AssignmentOp.Increment:
                    return Evaluate(Op.Plus, lhs, 1);
                case AssignmentOp.Decrement:
                    return Evaluate(Op.Minus, lhs, 1);
                default:
                    break;
            }

            return Evaluate((Op)op, lhs, rhs);
        }

        public object EvaluateObject(object value)
        {
            if (value is IProcessor)
                return (value as IProcessor).Process(this);
            if (value is ResourceObject)
                return (value as ResourceObject).Value;
            return value;
        }

        public object Evaluate(DependencyProperty property)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            return EvaluateObject(parent.GetValue(property));
        }

        public object Evaluate(DependencyProperty property, string path, PathExpression pathExpression)
        {
            return Evaluate(property, path, pathExpression, null);
        }

        public object Evaluate(DependencyProperty property, string path, PathExpression pathExpression, Type type)
        {
            var value = (path != null) ? GetPath(path, pathExpression) : Evaluate(property);
            return TypeHelper.Convert(value, type);
        }

        public Type EvaluateType(DependencyProperty property, string name)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            if (name == null && !PathHelper.HasBindingOrValue(parent, property)) return null;
            var type = name != null ? LookupType(name) : parent.GetValue(property) as Type;
            if (type != null) return type;
            return Throw("missing type") as Type;
        }

        public object Quote(DependencyProperty property)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            return parent.GetValue(property);
        }

        public void Execute(IEnumerable<IStatement> statements)
        {
            foreach (var statement in statements)
            {
                statement.Execute(this);
                if (ShouldInterrupt) break;
            }
        }

        public object CallFunction(string name, IEnumerable<object> args)
        {
            Trace(TraceFlags.Call, "CallFunction: " + name);
            if (name.StartsWith("@"))
            {
                var builtinFunction = default(BuiltinFunction);
                if (!Enum.TryParse<BuiltinFunction>(name.Substring(1), out builtinFunction)) Throw("invalid builtin function: " + name);
                return CallBuiltinFunction(builtinFunction, args);
            }
            var function = LookupFunction(name);
            if (function.HasParamsParameter)
            {
                var m = function.Parameters.Count - 1;
                DefineParameters(function.Parameters.Take(m), args.Take(m));
                DefineParameter(function.Parameters[m], args.Skip(m).ToArray());
            }
            else
                DefineParameters(function.Parameters, args);
            SetScopeFrame();
            SetReturnFrame();
            function.Body.Execute(this);
            return GetAndResetReturnValue();
        }

        private void DefineParameters(IEnumerable<Parameter> parameters, IEnumerable<object> args)
        {
            foreach (var pair in parameters.Zip(args, (parameter, argument) => Tuple.Create(parameter, argument)))
                DefineParameter(pair.Item1, pair.Item2);
        }

        private void DefineParameter(Parameter parameter, object arg)
        {
            DefineVariable("$" + parameter.Param, arg);
        }

        public object CallBuiltinFunction(BuiltinFunction builtinFunction, IEnumerable<object> args)
        {
            return CallHelper.CallMethod(builtinFunction.ToString(), false, typeof(BuiltinImplementor), BuiltinImplementor, args, null, this);
        }

        public ResourceObjectBase ParentResourceObject
        {
            get
            {
                var bestCaller = null as ResourceObjectBase;
                foreach (var frame in StackBackwards)
                {
                    var caller = frame.Caller;
                    if (caller is ResourceObjectBase) bestCaller = caller as ResourceObjectBase;
                    else if (bestCaller != null) return bestCaller;
                }
                return bestCaller;
            }
        }
    }
}
