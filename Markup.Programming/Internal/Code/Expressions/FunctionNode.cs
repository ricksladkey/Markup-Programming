using System.Linq;
using System.Collections.Generic;
using System;

namespace Markup.Programming.Core
{
    public class FunctionNode : CallNode
    {
        public string FunctionName { get; set; }
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            return engine.CallFunction(FunctionName, GetArguments(engine, args));
        }
    }
}
