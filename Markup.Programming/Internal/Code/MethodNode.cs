using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class MethodNode : CallNode
    {
        public ExpressionNode Callee { get; set; }
        public string MethodName { get; set; }
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            var context = Callee.Evaluate(engine, UnsetValue.Value);
            return CallHelper.CallMethod(MethodName, false, context.GetType(), context, GetArguments(engine, args), null, engine);
        }
    }
}
