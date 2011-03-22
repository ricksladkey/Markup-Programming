using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The BuiltinImplementor class implements functions that can be called
    /// natively from markup programming.
    /// </summary>
    public class BuiltinImplementor
    {
        private Engine engine;

        public BuiltinImplementor(Engine engine)
        {
            this.engine = engine;
        }

        public bool TryGetVariable(string name, out object value)
        {
            if (name == Engine.ContextKey)
            {
                var firstFrame = engine.Stack.First();
                if (firstFrame.Caller is ResourceComponent) value = firstFrame.Caller;
                else if (firstFrame.Caller.AssociatedObject is FrameworkElement)
                value = (firstFrame.Caller.AssociatedObject as FrameworkElement).DataContext;
                else value = engine.Throw("cannot locate default context");
                return true;
            }
            if (name == Engine.AssociatedObjectKey)
            {
                value = engine.CurrentFrame.Caller.AssociatedObject;
                return true;
            }
            if (name == Engine.SenderKey) { value = engine.Sender; return true; }
            if (name == Engine.EventArgsKey) { value = engine.EventArgs; return true; }
            value = null;
            return false;
        }

        public bool ParameterIsDefined(string name)
        {
            var value = null as object;
            return engine.TryGetVariable(name, out value);
        }

        public bool FunctionIsDefined(string name)
        {
            var value = null as Function;
            return engine.TryGetFunction(name, out value);
        }

        public object Evaluate(IExpression expression)
        {
            return expression.Evaluate(engine);
        }

        public ResourceComponent GetResourceObject()
        {
            return engine.ParentResourceObject;
        }

        public object Convert(object value, Type type)
        {
            return TypeHelper.Convert(value, type);
        }

        public object Op(Op op, params object[] operands)
        {
            return engine.Evaluate(op, operands);
        }
    }
}
