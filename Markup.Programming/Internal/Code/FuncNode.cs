using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class FuncNode : StatementNode, IFunction
    {
        public string FunctionName { get; set; }
        public ParameterCollection Parameters { get; set; }
        public bool HasParamsParameter { get; set; }
        public StatementNode Body { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.DefineFunction(FunctionName, this);
        }
        public void ExecuteBody(Engine engine)
        {
            Body.Execute(engine);
        }
    }
}
