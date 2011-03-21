using System;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class StaticMethodNode : CallNode
    {
        public TypeNode Type { get; set; }
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            var type = Type.Evaluate(engine, null) as Type;
            return CallHelper.CallMethod(Name, true, type, null, GetArguments(engine, args), null, engine);
        }
    }
}
