﻿using System.Collections.Generic;

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
