using System;
using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class TypeNode : ExpressionNode
    {
        public string TypeName { get; set; }
        public IList<TypeNode> TypeArguments { get; set; }
        protected override object OnGet(Engine engine)
        {
            var type = engine.GetType(TypeName);
            if (TypeArguments == null || TypeArguments.Count == 0) return type;
            var typeArgs = TypeArguments.Select(arg => arg.Get(engine)).Cast<Type>().ToArray();
            return type.MakeGenericType(typeArgs);
        }
    }
}
