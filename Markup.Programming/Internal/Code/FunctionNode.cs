using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class FunctionNode : CallNode
    {
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            return engine.CallFunction(Name, GetArguments(engine, args));
        }
    }
}
