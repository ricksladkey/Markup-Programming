using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Markup.Programming.Core
{
    public static class MethodHelper
    {
        public static object CallMethod(string methodName, MethodInfo methodInfo, object callee, object[] args, Engine engine)
        {

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
