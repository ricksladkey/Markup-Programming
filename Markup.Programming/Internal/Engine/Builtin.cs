using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The Builtin class contains functions that can be called
    /// natively from markup programming.
    /// </summary>
    public class Builtin
    {
        private Engine engine;

        public Builtin(Engine engine)
        {
            this.engine = engine;
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
