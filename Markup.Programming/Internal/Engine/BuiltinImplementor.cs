using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The BuiltinImplementor class implements functions that can be called
    /// natively from markup programming.
    /// </summary>
    public class BuiltinImplementor
    {
        public static IDictionary<string, object> ConstantParameters = new NameDictionary
        {
            { "@null", null },
            { "@true", true },
            { "@false", false },
        };
        private Engine engine;

        public BuiltinImplementor(Engine engine)
        {
            this.engine = engine;
        }

        public bool TryLookupParameter(string name, out object value)
        {
            if (name == "@AssociatedObject")
            {
                value = engine.CurrentFrame.Caller.AssociatedObject;
                return true;
            }
            if (name == "@Sender") { value = engine.Sender; return true; }
            if (name == "@EventArgs") { value = engine.EventArgs; return true; }
            if (ConstantParameters.ContainsKey(name)) { value = ConstantParameters[name]; return true; }
            value = null;
            return false;
        }

        public bool ParameterIsDefined(string name)
        {
            var value = null as object;
            return engine.TryLookupParameter(name, out value);
        }

        public bool FunctionIsDefined(string name)
        {
            var value = null as Function;
            return engine.TryLookupFunction(name, out value);
        }

        public object Evaluate(IExpression expression)
        {
            return expression.Evaluate(engine);
        }

        public ResourceObject GetResourceObject()
        {
            return engine.ParentResourceObject;
        }
    }
}
