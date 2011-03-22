using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public abstract class CallNode : ExpressionNode
    {
        public IList<ExpressionNode> Arguments { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return Call(engine, GetArguments(engine, null));
        }
        protected IEnumerable<object> GetArguments(Engine engine, IEnumerable<object> args)
        {
            if (Arguments != null) return Arguments.Select(argument => argument.Evaluate(engine)).ToArray();
            if (args == null) engine.Throw("missing arguments");
            return args;
        }
        public object Call(Engine engine, IEnumerable<object> args)
        {
            return OnCall(engine, args);
        }
        public abstract object OnCall(Engine engine, IEnumerable<object> args);
    }
}
