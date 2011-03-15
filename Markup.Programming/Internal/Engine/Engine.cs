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
        Parameter = 0x8,
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
    /// It is lightweight enough to be created and discard at will.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Frames = {stack.Count}"), DebuggerTypeProxy(typeof(EngineDebugView))]
#endif
    public class Engine
    {
        public static string ContextParameter = "@";

        private static int nextId = 0;
        private int id;
        private TraceFlags TraceFilter = Configuration.TraceDefaults;
        private bool shouldBreak;
        private bool shouldReturn;
        private object returnValue;
        private List<StackFrame> stack = new List<StackFrame>();
        private IDictionary<string, Function> functions;
        private StackFrame CurrentFrame { get { return stack.Count >= 1 ? stack[stack.Count - 1] : null; } }
        private StackFrame ParentFrame { get { return stack.Count >= 2 ? stack[stack.Count - 2] : null; } }
        private string FrameInfo { get { return string.Format("{0}/{1}", id, stack.Count); } }

        public Engine()
            : this(null, null)
        {
        }

        public Engine(object parameter)
            : this(null, parameter as EventArgs)
        {
        }

        public Engine(object sender, object e)
        {
            id = nextId++;
            Sender = sender;
            EventArgs = e;
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

            // Attempt to set the default context.
            if (stack.Count == 1)
            {
                if (CurrentFrame.Caller is ResourceObject)
                {
                    DefineParameter(ContextParameter, CurrentFrame.Caller);
                    return;
                }
                var context = CurrentFrame.Caller.AssociatedObject;
                if (context is FrameworkElement)
                {
                    DefineParameter(ContextParameter, (context as FrameworkElement).DataContext);
                    return;
                }
            }
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

        public void With(IComponent caller, Action<Engine> statement)
        {
            PushFrame(caller);
            statement(this);
            PopFrame();
        }

        public void With(IComponent caller, IDictionary<string, object> dictionary, Action<Engine> statement)
        {
            PushFrame(caller);
            DefineParameters(dictionary);
            statement(this);
            PopFrame();
        }

        public TResult With<TResult>(IComponent caller, Func<Engine, TResult> func)
        {
            PushFrame(caller);
            var result = func(this);
            PopFrame();
            return result;
        }

        public TResult With<TResult>(IComponent caller, IDictionary<string, object> dictionary, Func<Engine, TResult> func)
        {
            PushFrame(caller);
            DefineParameters(dictionary);
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
            ThrowHelper.Throw("missing yield frame");
        }

        public IList<object> GetYieldedValues()
        {
            if (CurrentFrame.YieldedValues == null) ThrowHelper.Throw("missing yielded values");
            return CurrentFrame.YieldedValues;
        }

        public void DefineParameter(string name, object value)
        {
            Trace(TraceFlags.Parameter, "Define: {0} = {1}", name, value);
            DefineParameter(name, value, false);
        }

        public void DefineParameters(IEnumerable<Parameter> parameters, IEnumerable<object> args)
        {
            foreach (var pair in parameters.Zip(args,
                (parameter, argument) => Tuple.Create(parameter.ParameterName, argument)))
                DefineParameter(pair.Item1, pair.Item2);
        }

        public void DefineParameters(IDictionary<string, object> dictionary)
        {
            foreach (var pair in dictionary) DefineParameter(pair.Key, pair.Value, false);
        }

        public void DefineParameterInParentScope(string name, object value)
        {
            DefineParameter(name, value, true);
        }

        private void DefineParameter(string name, object value, bool parentFrame)
        {
            Trace(TraceFlags.Parameter, "DefineParameter: {0} = {1}", name, value);
            var frame = parentFrame ? (ParentFrame ?? CurrentFrame) : CurrentFrame;
            if (frame == null) ThrowHelper.Throw("no frame for parameter: " + name);
            if (frame.Parameters == null) frame.Parameters = new NameDictionary();
            frame.Parameters[name] = value;
        }

        public bool TryLookupParameter(string name, out object value)
        {
            foreach (var frame in StackBackwards)
            {
                if (frame.Parameters != null && frame.Parameters.ContainsKey(name))
                {
                    value = frame.Parameters[name];
                    return true;
                }
                if (frame.ScopeFrame) break;
            }
            if (name == "AssociatedObject")
            {
                if (CurrentFrame == null) ThrowHelper.Throw("no frame for AssociatedObject");
                if (CurrentFrame.Caller == null) ThrowHelper.Throw("no caller for AssociatedObject");
                value = CurrentFrame.Caller.AssociatedObject;
                return true;
            }
            if (name == "Sender") { value = Sender; return true; }
            if (name == "EventArgs") { value = EventArgs; return true; }
            value = null;
            return false;
        }

        public object LookupParameter(string name)
        {
            var value = null as object;
            if (TryLookupParameter(name, out value))
            {
                Trace(TraceFlags.Parameter, "Lookup: {0} = {1}", name, value);
                return value;
            }
            return ThrowHelper.Throw("parameter not found: " + name);
        }

        public void DefineFunction(string name, Function value)
        {
            if (functions == null) functions = new Dictionary<string, Function>();
            if (functions.ContainsKey(name) && functions[name] != value)
                ThrowHelper.Throw("function redefinition: " + name);
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
            return ThrowHelper.Throw("function not found: " + name) as Function;
        }

        public static Type LookupType(string name)
        {
            if (name.Split(',').Length > 2) return TypeHelper.ConvertToType(name);
            return TypeHelper.ResolvePartialType(name);
        }

        public object Context
        {
            get
            {
                var value = null as object;
                if (TryLookupParameter(ContextParameter, out value)) return value;
                return null;
            }
        }

        public object GetContext(string path)
        {
            return path != null ? GetPath(path) : Context;
        }

        public void SetContext(DependencyProperty property, string path)
        {
            var caller = CurrentFrame.Caller as DependencyObject;
            if (caller.GetValue(property) != null || path != null || PathHelper.HasBinding(caller, property))
            {
                var context = Evaluate(property, path);
                Trace(TraceFlags.Parameter, "Setting context = {0}", context);
                DefineParameter(Engine.ContextParameter, context);
            }
        }

        public object GetPath(string path)
        {
            return new PathExpression(true, true, path).Evaluate(this);
        }

        public object SetPath(string path, object value)
        {
            return new PathExpression(false, true, path).Evaluate(this, value);
        }

        public bool ShouldTrace(TraceFlags flags)
        {
            return (flags & TraceFilter) != 0;
        }

        [Conditional("TRACE")]
        public void Trace(TraceFlags flags, string format, params object[] parameters)
        {
            if (ShouldTrace(flags)) TraceHelper.WriteLine(format, parameters);
        }

        [Conditional("TRACE")]
        private void TraceStack(string format, params object[] args)
        {
            Trace(TraceFlags.Stack, "[" + FrameInfo + "]" + format, args);
        }

        public object Evaluate(Operator op, params object[] values)
        {
            return OperatorHelper.Evaluate(op, ExpressionOrValue.ValueArray(values), this);
        }

        public object Evaluate(Operator op, IEnumerable<IExpression> collection)
        {
            return OperatorHelper.Evaluate(op, ExpressionOrValue.ExpressionArray(collection), this);
        }

        public object Evaluate(Operator op, ExpressionOrValue[] expressions)
        {
            return OperatorHelper.Evaluate(op, expressions, this);
        }

        public object Evaluate(Operator op, ExpressionCollection collection)
        {
            return OperatorHelper.Evaluate(op, ExpressionOrValue.ExpressionArray(collection), this);
        }

        public object Evaluate(AssignmentOperator op, object lhs, object rhs)
        {
            switch (op)
            {
                case AssignmentOperator.Assign:
                    return rhs;
                case AssignmentOperator.Increment:
                    return Evaluate(Operator.Plus, lhs, 1);
                case AssignmentOperator.Decrement:
                    return Evaluate(Operator.Minus, lhs, 1);
                default:
                    break;
            }

            return Evaluate((Operator)op, lhs, rhs);
        }

        public object EvaluateObject(object value)
        {
            if (value is IProcessor)
                return (value as IProcessor).Process(this);
            if (value is MarkupObject)
                return (value as MarkupObject).Value;
            return value;
        }

        public object Evaluate(DependencyProperty property)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            return EvaluateObject(parent.GetValue(property));
        }

        public object Evaluate(DependencyProperty property, string path)
        {
            return Evaluate(property, path, null);
        }

        public object Evaluate(DependencyProperty property, string path, Type type)
        {
            var value = (path != null) ? GetPath(path) : Evaluate(property);
            return TypeHelper.Convert(type, value);
        }

        public Type EvaluateType(DependencyProperty property, string name)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            if (name == null && !PathHelper.HasBindingOrValue(parent, property)) return null;
            var type = name != null ? LookupType(name) : parent.GetValue(property) as Type;
            if (type != null) return type;
            return ThrowHelper.Throw("missing type") as Type;
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
            var function = LookupFunction(name);
            if (function.HasParamsParameter)
            {
                var m = function.Parameters.Count - 1;
                DefineParameters(function.Parameters.Take(m), args.Take(m));
                DefineParameter(function.Parameters[m].ParameterName, args.Skip(m).ToArray());
            }
            else
                DefineParameters(function.Parameters, args);
            SetScopeFrame();
            SetReturnFrame();
            function.Body.Execute(this);
            return GetAndResetReturnValue();
        }

        public ResourceObject ParentResourceObject
        {
            get
            {
                var bestCaller = null as ResourceObject;
                foreach (var frame in StackBackwards)
                {
                    var caller = frame.Caller;
                    if (caller is ResourceObject) bestCaller = caller as ResourceObject;
                    else if (bestCaller != null) return bestCaller;
                }
                return bestCaller;
            }
        }
    }
}
