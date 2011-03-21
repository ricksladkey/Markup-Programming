using System;
using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class TypeNode : ExpressionNode
    {
        public IList<TypeNode> TypeArguments { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            var type = engine.LookupType(Name);
            if (TypeArguments == null || TypeArguments.Count == 0) return type;
            var typeArgs = TypeArguments.Select(arg => arg.Evaluate(engine, value)).Cast<Type>().ToArray();
            return type.MakeGenericType(typeArgs);
        }
    }
}
