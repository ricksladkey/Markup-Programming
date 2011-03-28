using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

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

        public bool VariableIsDefined(string name)
        {
            var value = null as object;
            return engine.TryGetVariable(name, out value);
        }

        public bool FunctionIsDefined(string name)
        {
            var value = null as IFunction;
            return engine.TryGetFunction(name, out value);
        }

        public object Evaluate(IExpression expression)
        {
            return expression.Get(engine);
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
            return engine.Operator(op, operands);
        }

        public object FindElement(string elementName)
        {
            return FindElement(elementName, null);
        }

        public object FindElement(string elementName, DependencyObject context)
        {
            if (context == null) context = engine.CurrentFrame.Caller.AssociatedObject;
            while (context != null)
            {
                if (context is FrameworkElement)
                {
                    var element = (context as FrameworkElement).FindName(elementName);
                    if (element != null) return element;
                }
                context = VisualTreeHelper.GetParent(context);
            }
            return null;
        }

        public object FindAncestor(Type type)
        {
            return FindAncestor(type, null);
        }

        public object FindAncestor(Type type, DependencyObject context)
        {
            if (context == null) context = engine.CurrentFrame.Caller.AssociatedObject;
            while (context != null)
            {
                if (context.GetType().IsAssignableFrom(type)) return context;
                context = VisualTreeHelper.GetParent(context);
            }
            return null;
        }
    }
}
