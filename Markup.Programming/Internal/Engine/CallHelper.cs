using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Markup.Programming.Core
{
    public static class CallHelper
    {
        public static object Call(PathExpression pathExpression, string path, string staticMethodName, string methodName, string functionName, BuiltinFunction builtinFunction, Type type, ExpressionCollection typeArguments, object[] args, Engine engine)
        {
            if (path != null)
                return engine.CallPath(pathExpression, path, args);
            if (functionName != null)
                return engine.CallFunction(functionName, args);
            if (builtinFunction != 0)
                return engine.CallBuiltinFunction(builtinFunction, args);
            var typeArgs = typeArguments.Count != 0 ? typeArguments.Evaluate(engine).Cast<Type>().ToArray() : null;
            if (staticMethodName != null)
                return CallHelper.CallMethod(staticMethodName, true, type, null, args, typeArgs, engine);
            if (methodName != null)
                return CallHelper.CallMethod(methodName, false, type ?? engine.Context.GetType(), engine.Context, args, typeArgs, engine);
            return engine.Throw("nothing to call");
        }

        public static object CallMethod(string methodName, bool staticMethod, Type typeToCall, object callee, object[] args, Type[] typeArgs, Engine engine)
        {
            var bindingFlags = (staticMethod ? BindingFlags.Static : BindingFlags.Instance) |
                BindingFlags.Public | BindingFlags.FlattenHierarchy;
            var methodInfo = null as MethodInfo;
            if (typeArgs != null)
            {
                // Use type arguments to choose overload.
                methodInfo = typeToCall.GetMethod(methodName, bindingFlags, null, typeArgs, null);
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
                    if (methodInfo == null) engine.Throw("method overload not found: " + methodName);
                }
            }
            return CallHelper.CallMethod(methodName, methodInfo, callee, args, engine);
        }

        public static object CallMethod(string methodName, MethodInfo methodInfo, object callee, object[] args, Engine engine)
        {

            if (methodInfo == null) engine.Throw("method not found: " + methodName);
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
                if (parameters.Length != args.Length) engine.Throw("argument count mismatch: {0} != {1}", parameters.Length, args.Length);
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

        private static void TraceCall(string info, string methodName, object[] args, Engine engine)
        {
            var text = args.Length == 0 ? "" : args.Aggregate((current, next) => current + ", " + next);
            engine.Trace(TraceFlags.Call, "Call {0}: {1}({2})", info, methodName, text);
        }

        private static IEnumerable<object> ConvertArguments(IEnumerable<ParameterInfo> parameters, IEnumerable<object> args)
        {
            return parameters.Zip(args, (parameterInfo, arg) => TypeHelper.Convert(parameterInfo.ParameterType, arg));
        }

        public static bool HasParamsParameter(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 &&
                parameters[parameters.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }
    }
}
