﻿using System;
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
        private bool shouldContinue;
        private bool shouldReturn;
        private object returnValue;
        private int scriptDepth;
        private List<StackFrame> stack = new List<StackFrame>();
        private IDictionary<string, IFunction> functions;

        private string FrameInfo { get { return string.Format("{0}/{1}", id, stack.Count); } }

        public int ScriptDepth { get { return scriptDepth; } }
        public StackFrame CurrentFrame { get { return stack.Count >= 1 ? stack[stack.Count - 1] : null; } }
        private StackFrame ParentFrame { get { return stack.Count >= 2 ? stack[stack.Count - 2] : null; } }

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
        public bool ShouldInterrupt { get { return shouldContinue || shouldBreak || shouldReturn; } }
        public IEnumerable<StackFrame> Stack { get { return stack; } }
        public IEnumerable<StackFrame> StackBackwards
        {
            get { for (int i = stack.Count - 1; i >= 0; i--) yield return stack[i]; }
        }

        public string GetName(IComponent component)
        {
            return "p:" + component.GetType().Name;
        }

        private void PushFrame(IComponent caller, Node node)
        {
            stack.Add(new StackFrame(this, caller, node));
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

        public void FrameAction(IComponent caller, IDictionary<string, object> variables, Action<Engine> action)
        {
            PushFrame(caller, null);
            CurrentFrame.Variables = variables;
            action(this);
            PopFrame();
        }

        public void FrameAction(IComponent caller, Action<Engine> action)
        {
            PushFrame(caller, null);
            action(this);
            PopFrame();
        }

        public void FrameAction(Node node, Action<Engine> action)
        {
            ++scriptDepth;
            PushFrame(CurrentFrame.Caller, node);
            action(this);
            PopFrame();
            --scriptDepth;
        }

        public TResult FrameFunc<TResult>(IComponent caller, IDictionary<string, object> variables, Func<Engine, TResult> func)
        {
            PushFrame(caller, null);
            CurrentFrame.Variables = variables;
            var result = func(this);
            PopFrame();
            return result;
        }

        public TResult FrameFunc<TResult>(IComponent caller, Func<Engine, TResult> func)
        {
            PushFrame(caller, null);
            var result = func(this);
            PopFrame();
            return result;
        }

        public TResult FrameFunc<TResult>(Node node, Func<Engine, TResult> func)
        {
            ++scriptDepth;
            PushFrame(CurrentFrame.Caller, node);
            var result = func(this);
            PopFrame();
            --scriptDepth;
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

        public void SetShouldContinue()
        {
            shouldContinue = true;
        }

        public void ClearShouldContinue()
        {
            shouldContinue = false;
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
            DefineVariable(name, value, false, false);
        }

        public object DefineVariableInParentScope(string name, object value)
        {
            DefineVariable(name, value, true, false);
            return value;
        }

        private void DefineVariable(string name, object value, bool parentScope, bool noError)
        {
            if (!noError && !CodeTree.IsValidVariable(name)) Throw("invalid variable: " + name);
            Trace(TraceFlags.Variable, "DefineVariable: {0} = {1}", name, value);
            var frame = parentScope ? (ParentFrame ?? CurrentFrame) : CurrentFrame;
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

        public bool TryGetVariable(string name, out object value)
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
            return BuiltinImplementor.TryGetVariable(name, out value);
        }

        public object GetVariable(string name)
        {
            return GetVariable(name, false);
        }

        public object GetVariableInParentScope(string name)
        {
            return GetVariable(name, true);
        }

        private object GetVariable(string name, bool parentScope)
        {
            var value = null as object;
            if (TryGetVariable(name, out value))
            {
                Trace(TraceFlags.Variable, "Get: {0} = {1}", name, value);
                return value;
            }
            var frame = parentScope ? (ParentFrame ?? CurrentFrame) : CurrentFrame;
            if (frame.Variables == null) frame.Variables = new NameDictionary();
            frame.Variables[name] = null;
            return null;
        }

        public object SetVariable(string name, object value)
        {
            return SetVariable(name, value, false);
        }

        public object SetVariableInParentScope(string name, object value)
        {
            return SetVariable(name, value, true);
        }

        private object SetVariable(string name, object value, bool parentScope)
        {
            foreach (var frame in StackBackwards)
            {
                if (frame.Variables != null && frame.Variables.ContainsKey(name))
                {
                    frame.Variables[name] = value;
                    return value;
                }
                if (frame.ScopeFrame) break;
            }
            {
                var frame = parentScope ? (ParentFrame ?? CurrentFrame) : CurrentFrame;
                if (frame.Variables == null) frame.Variables = new NameDictionary();
                frame.Variables[name] = value;
            }
            return value;
        }

        public void DefineFunction(string name, IFunction value)
        {
            if (functions == null) functions = new Dictionary<string, IFunction>();
            if (functions.ContainsKey(name) && functions[name] != value)
                Throw("function redefinition: " + name);
            functions[name] = value;
        }

        public bool TryGetFunction(string name, out IFunction value)
        {
            if (functions != null && functions.ContainsKey(name))
            {
                value = functions[name];
                return true;
            }
            value = null;
            return false;
        }

        public IFunction GetFunction(string name)
        {
            var value = null as IFunction;
            if (TryGetFunction(name, out value)) return value;
            return Throw("function not found: " + name) as IFunction;
        }

        public Type GetType(string name)
        {
            var result = name.Split(',').Length > 2 ? TypeHelper.ConvertToType(name) : TypeHelper.ResolvePartialType(name);
            if (result == null) Throw("unable to resolve type: " + name);
            return result;
        }

        public object Context
        {
            get
            {
                var value = null as object;
                if (TryGetVariable(ContextKey, out value)) return value;
                return null;
            }
        }

        public object GetContext(string path, CodeTree codeTree)
        {
            return path != null ? GetPath(path, codeTree) : Context;
        }

        public void SetContext(DependencyProperty property, string path, CodeTree codeTree)
        {
            if (HasBindingOrValue(property, path))
                SetContext(Get(property, path, codeTree));
        }

        public void SetContext(object context)
        {
            DefineVariable(Engine.ContextKey, context, false, true);
        }

        public bool HasBindingOrValue(DependencyProperty property)
        {
            return HasBindingOrValue(property, null);
        }

        public bool HasBindingOrValue(DependencyProperty property, string path)
        {
            var caller = CurrentFrame.Caller as DependencyObject;
            return PathHelper.HasBindingOrValue(caller, property, path);
        }

        public string GetVariable(string path, CodeTree codeTree)
        {
            if (codeTree == null) codeTree = new CodeTree();
            return codeTree.Compile(this, CodeType.Variable, path).GetVariable(this);
        }

        public string GetEvent(string path, CodeTree codeTree)
        {
            if (codeTree == null) codeTree = new CodeTree();
            return codeTree.Compile(this, CodeType.Event, path).GetEvent(this);
        }

        public object GetPath(string path, CodeTree codeTree)
        {
            if (codeTree == null) codeTree = new CodeTree();
            return codeTree.Compile(this, CodeType.Get, path).Get(this);
        }

        public void ExecuteScript(string path, CodeTree codeTree)
        {
            if (codeTree == null) codeTree = new CodeTree();
            codeTree.Compile(this, CodeType.Script, path).Execute(this);
        }

        public object SetPath(string path, CodeTree codeTree, object value)
        {
            if (codeTree == null) codeTree = new CodeTree();
            return codeTree.Compile(this, CodeType.Set, path).Set(this, value);
        }

        public object CallPath(string path, CodeTree codeTree, IEnumerable<object> args)
        {
            if (codeTree == null) codeTree = new CodeTree();
            return codeTree.Compile(this, CodeType.Call, path).Call(this, args);
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
            var formattedMessage = args.Length == 0 ? message : string.Format(message, args);
                return ThrowHelper.Throw(new InvalidOperationException(formattedMessage), this);
        }

        public object Operator(Op op, params object[] values)
        {
            return OperatorHelper.Operator(this, op, ExpressionOrValue.ValueArray(values));
        }

        public object Operator(Op op, IEnumerable<IExpression> collection)
        {
            return OperatorHelper.Operator(this, op, ExpressionOrValue.ExpressionArray(collection));
        }

        public object Operator(Op op, ExpressionOrValue[] expressions)
        {
            return OperatorHelper.Operator(this, op, expressions);
        }

        public object Operator(Op op, ExpressionCollection collection)
        {
            return OperatorHelper.Operator(this, op, ExpressionOrValue.ExpressionArray(collection));
        }

        public object Operator(AssignmentOp op, object lhs, object rhs)
        {
            switch (op)
            {
                case AssignmentOp.Assign:
                    return rhs;
                case AssignmentOp.Increment:
                    return Operator(Op.Plus, lhs, 1);
                case AssignmentOp.Decrement:
                    return Operator(Op.Minus, lhs, 1);
                default:
                    break;
            }

            return Operator((Op)op, lhs, rhs);
        }

        public object GetExpression(object value)
        {
            if (value is IProcessor)
                return (value as IProcessor).Process(this);
            if (value is ResourceObject)
                return (value as ResourceObject).Value;
            return value;
        }

        public object Get(DependencyProperty property)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            return GetExpression(parent.GetValue(property));
        }

        public object Get(DependencyProperty property, string path, CodeTree codeTree)
        {
            return Get(property, path, codeTree, null);
        }

        public object Get(DependencyProperty property, string path, CodeTree codeTree, Type type)
        {
            var value = (path != null) ? GetPath(path, codeTree) : Get(property);
            return TypeHelper.Convert(value, type);
        }

        public Type GetType(DependencyProperty property, string path, CodeTree codeTree)
        {
            return Get(property, path, codeTree, typeof(Type)) as Type;
        }

        public object Quote(DependencyProperty property)
        {
            var parent = CurrentFrame.Caller as DependencyObject;
            return parent.GetValue(property);
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
            var function = GetFunction(name);
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
            function.ExecuteBody(this);
            return GetAndResetReturnValue();
        }

        private void DefineParameters(IEnumerable<Parameter> parameters, IEnumerable<object> args)
        {
            foreach (var pair in parameters.Zip(args, (parameter, argument) => Tuple.Create(parameter, argument)))
                DefineParameter(pair.Item1, pair.Item2);
        }

        private void DefineParameter(Parameter parameter, object arg)
        {
            DefineVariable(parameter.ParameterName, arg);
        }

        public object CallBuiltinFunction(BuiltinFunction builtinFunction, IEnumerable<object> args)
        {
            return CallHelper.CallMethod(builtinFunction.ToString(), false, typeof(BuiltinImplementor), BuiltinImplementor, args, null, this);
        }

        public ResourceComponent ParentResourceObject
        {
            get
            {
                var bestCaller = null as ResourceComponent;
                foreach (var frame in StackBackwards)
                {
                    var caller = frame.Caller;
                    if (caller is ResourceComponent) bestCaller = caller as ResourceComponent;
                    else if (bestCaller != null) return bestCaller;
                }
                return bestCaller;
            }
        }
    }
}
