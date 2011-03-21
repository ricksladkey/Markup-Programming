using System.Collections;

namespace Markup.Programming.Core
{
    public class ForEachNode : FrameNode
    {
        public ExpressionNode Collection { get; set; }
        public string VariableName { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            foreach (var item in Collection.Evaluate(engine) as IEnumerable)
            {
                engine.DefineVariable(VariableName, item);
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
