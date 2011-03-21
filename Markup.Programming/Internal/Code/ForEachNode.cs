using System.Collections;

namespace Markup.Programming.Core
{
    public class ForEachNode : BodyNode
    {
        public string Name { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            engine.SetBreakFrame();
            foreach (var item in Context.Evaluate(engine) as IEnumerable)
            {
                engine.DefineVariable(Name, item);
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
