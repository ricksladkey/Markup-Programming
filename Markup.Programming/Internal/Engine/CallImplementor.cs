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
            if (caller.PathBase != null) return engine.CallPath(caller.PathBase, args);
            if (caller.FunctionName != null) return engine.CallFunction(caller.FunctionName, args);
            if (caller.BuiltinFunction != 0) return CallBuiltinFunction(engine, args);
            return CallMethod(engine, args);
        }

        private object CallBuiltinFunction(Engine engine, object[] args)
        {
            var builtin = new BuiltinImplementor(engine);
            return MethodHelper.CallMethod(caller.BuiltinFunction.ToString(), false, typeof(BuiltinImplementor), builtin, args, null, engine);
        }

        private object CallMethod(Engine engine, object[] args)
        {
            var typeArgs = caller.TypeArguments.Count != 0 ?
                caller.TypeArguments.Evaluate(engine).Cast<Type>().ToArray() : null;
            if (caller.StaticMethodName != null)
            {
                var type = engine.EvaluateType(caller.CallerTypeProperty, caller.TypeName);
                return MethodHelper.CallMethod(caller.StaticMethodName, true, type, null, args, typeArgs, engine);
            }
            if (caller.MethodName != null)
            {
                var context = engine.GetContext(caller.PathBase);
                var typeToCall = caller.Type ?? context.GetType();
                return MethodHelper.CallMethod(caller.MethodName, false, typeToCall, context, args, typeArgs, engine);
            }
            return ThrowHelper.Throw("nothing to call");
        }
    }
}
