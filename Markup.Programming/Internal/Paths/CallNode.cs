using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public abstract class CallNode : PathNode
    {
        public IList<PathNode> Arguments { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return Call(engine, GetArguments(engine, null));
        }
        protected IEnumerable<object> GetArguments(Engine engine, IEnumerable<object> args)
        {
            if (Arguments != null) return Arguments.Select(argument => argument.Evaluate(engine, UnsetValue.Value)).ToArray();
            if (args == null) engine.Throw("missing arguments");
            return args;
        }
        public abstract object Call(Engine engine, IEnumerable<object> args);
    }
}
