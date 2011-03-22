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
#if false
            var func = engine.GetVariable(FunctionName) as FuncNode;
            foreach (var pair in func.Parameters.Zip(args, (parameter, argument) => Tuple.Create(parameter.Param, argument)))
                engine.DefineVariable(pair.Item1, pair.Item2);
            engine.SetScopeFrame();
            engine.SetReturnFrame();
            func.Body.Execute(engine);
            return engine.GetAndResetReturnValue();
#else
            return engine.CallFunction(FunctionName, GetArguments(engine, args));
#endif
        }
    }
}
