using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A CallImplementor is an implementor for classes that themselves
    /// implement the ICall interface including Call and CallHandler.
    /// This design is required because those classes cannot share
    /// a common base class due to other restrictions.  As a result
    /// we use the bridge pattern and forward the implementation to
    /// this class.
    /// </summary>
    public class CallImplementor<T> where T : DependencyObject, ICaller
    {
        private T caller;

        internal CallImplementor(T caller)
        {
            this.caller = caller;
        }

        private bool ParameterSupplied
        {
            get
            {
                return caller.Parameter != null || caller.ParameterPath != null ||
                    PathHelper.HasBinding(caller, caller.CallerParameterProperty);
            }
        }

        public object Call(Engine engine)
        {
            var args = caller.Arguments.Evaluate(engine);
            if (ParameterSupplied)
            {
                var parameter = engine.Evaluate(caller.CallerParameterProperty, caller.ParameterPath);
                args = new object[] { engine.EvaluateObject(parameter) }.Concat(args).ToArray();
            }
            if (caller.FunctionName != null) return CallFunction(engine, args);
            if (caller.BuiltinFunction != 0) return CallBuiltinFunction(engine, args);
            return CallMethod(engine, args);
        }

        private object CallBuiltinFunction(Engine engine, object[] args)
        {
            var builtin = new BuiltinImplementor(engine);
            return CallMethod(caller.BuiltinFunction.ToString(), false, typeof(BuiltinImplementor), builtin, args, engine);
        }

        private object CallMethod(Engine engine, object[] args)
        {
            if (caller.StaticMethodName != null)
            {
                var type = engine.EvaluateType(caller.CallerTypeProperty, caller.TypeName);
                return CallMethod(caller.StaticMethodName, true, type, null, args, engine);
            }
            if (caller.MethodName != null)
            {
                var context = engine.GetContext(caller.PathBase);
                var typeToCall = caller.Type ?? context.GetType();
                return CallMethod(caller.MethodName, false, typeToCall, context, args, engine);
            }
            var path = caller.PathBase;
            if (path != null && path.Length > 0 && path[0] == ':')
            {
                var fields = ParseMethod(path.Substring(1), ref args, engine);
                return CallMethod(fields[1], true, engine.LookupType(fields[0]), null, args, engine);
            }
            if (path != null)
            {
                var fields = ParseMethod(path, ref args, engine);
                var context = engine.GetContext(fields[0]);
                var typeToCall = caller.Type ?? context.GetType();
                return CallMethod(fields[1], false, typeToCall, context, args, engine);
            }
            return ThrowHelper.Throw("nothing to call");
        }

        private static string[] ParseMethod(string path, ref object[] args, Engine engine)
        {
            var context = ".";
            var name = path;
            var parameters = "";
            int start = name.IndexOf('(');
            int end = name.LastIndexOf(')');
            if (start != -1 && end != -1)
            {
                parameters = name.Substring(start + 1, end - (start + 1)).Trim();
                if (parameters == "") args = new object[0];
                else args = parameters.Split(',').Select(parameter => engine.GetPath(parameter)).ToArray();
                name = name.Substring(0, start);
            }
            int n = name.LastIndexOf('.');
            if (n != -1)
            {
                context = name.Substring(0, n);
                name = name.Substring(n + 1);
            }
            return new string[] { context, name };
        }

        private object CallMethod(string methodName, bool staticMethod, Type typeToCall, object callee, object[] args, Engine engine)
        {
            var bindingFlags = (staticMethod ? BindingFlags.Static : BindingFlags.Instance) |
                BindingFlags.Public | BindingFlags.FlattenHierarchy;
            var methodInfo = null as MethodInfo;
            if (caller.TypeArguments.Count != 0)
            {
                // Use type arguments to choose overload.
                var types = caller.TypeArguments.Evaluate(engine).Cast<Type>().ToArray();
                methodInfo = typeToCall.GetMethod(methodName, bindingFlags, null, types, null);
            }
            else
            {
                try
                {
                    // Try default method.
                    methodInfo = typeToCall.GetMethod(methodName, bindingFlags);
                }
                catch
                {
                    // Use arguments to choose overload.
                    var types = args.Select(value => value != null ? value.GetType() : typeof(object));
                    methodInfo = typeToCall.GetMethod(methodName, bindingFlags, null, types.ToArray(), null);
                    if (methodInfo == null) ThrowHelper.Throw("method overload not found: " + methodName);
                }
            }
            if (methodInfo == null) ThrowHelper.Throw("method not found: " + methodName);
            var parameters = methodInfo.GetParameters();
            var methodArgs = null as object[];
            if (HasParamsParameter(parameters))
            {
                var m = parameters.Length - 1;
                var initiaArgs = ConvertArguments(parameters.Take(m), args.Take(m));
                var paramsType = parameters[m].ParameterType.GetElementType();
                var remainingArgs = TypeHelper.CreateArray(args.Skip(m).ToArray(), paramsType);
                methodArgs = initiaArgs.Concat(new object[] { remainingArgs }).ToArray();
            }
            else
            {
                if (parameters.Length != args.Length) ThrowHelper.Throw(string.Format("argument count mismatch: {0} != {1}", parameters.Length, args.Length));
                methodArgs = ConvertArguments(parameters, args).ToArray();
            }
            bool trace = engine.ShouldTrace(TraceFlags.Call);
            if (trace)
            {
                TraceCall("args", methodName, args, engine);
                TraceCall("methodArgs", methodName, methodArgs, engine);
            }
            var result = methodInfo.Invoke(callee, methodArgs);
            if (trace) engine.Trace(TraceFlags.Call, "Call {0} = {1}", methodName, result);
            return result;
        }

        private IEnumerable<object> ConvertArguments(IEnumerable<ParameterInfo> parameters, IEnumerable<object> args)
        {
            return parameters.Zip(args, (parameterInfo, arg) => TypeHelper.Convert(parameterInfo.ParameterType, arg));
        }

        private void TraceCall(string info, string methodName, object[] args, Engine engine)
        {
            var text = args.Length == 0 ? "" : args.Aggregate((current, next) => current + ", " + next);
            engine.Trace(TraceFlags.Call, "Call {0}: {1}({2})", info, methodName, text);
        }

        private object CallFunction(Engine engine, IEnumerable<object> args)
        {
            return engine.CallFunction(caller.FunctionName, args);
        }

        private static bool HasParamsParameter(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 &&
                parameters[parameters.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }
    }
}
