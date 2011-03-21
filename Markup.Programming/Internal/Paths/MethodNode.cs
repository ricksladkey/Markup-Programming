using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class MethodNode : CallNode
    {
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            var context = Context.Evaluate(engine, UnsetValue.Value);
            return CallHelper.CallMethod(Name, false, context.GetType(), context, GetArguments(engine, args), null, engine);
        }
    }
}
