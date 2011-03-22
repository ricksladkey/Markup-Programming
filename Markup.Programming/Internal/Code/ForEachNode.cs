using System.Collections;

namespace Markup.Programming.Core
{
    public class ForEachNode : FramedStatementNode
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
                engine.ClearShouldContinue();
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
